using Microsoft.EntityFrameworkCore;
using VlaDO.DTOs;
using VlaDO.Models;
using VlaDO.Repositories.Documents;

namespace VlaDO.Repositories
{
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(DocumentFlowContext context) : base(context) { }

        public async Task<IEnumerable<Document>> GetByCreatorAsync(Guid userId)
        {
            return await _context.Documents
                .Where(d => d.CreatedBy == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByRoomAsync(Guid roomId)
        {
            return await _context.Documents
                .Where(d => d.RoomId == roomId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByRoomAndUserAsync(Guid roomId, Guid userId)
        {
            return await _context.Documents
                .Where(d => d.RoomId == roomId && d.CreatedBy == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomBriefDto>> GetLastActiveRoomsAsync(Guid userId, int top = 3)
        {
            var q =
                from d in _context.Documents
                where d.CreatedBy == userId && d.RoomId != null
                group d by d.RoomId into g
                let last = g.Max(x => x.CreatedOn)
                orderby last descending
                select new RoomBriefDto(
                    g.Key!.Value,
                    g.Select(x => x.Room!.Title).FirstOrDefault() ?? "(без названия)",
                    last);

            return await q.Take(top).ToListAsync();
        }
        public async Task<IEnumerable<Document>> GetByRoomAndUserAsyncExcludeCreator(Guid userId)
        {
            // 1. Получаем Id всех комнат, где пользователь участник
            var accessibleRoomIds = await _context.RoomUsers
                .Where(ru => ru.UserId == userId)
                .Select(ru => ru.RoomId)
                .ToListAsync();

            // 2. Берем все документы, у которых:
            // - либо созданы этим пользователем
            // - либо находятся в доступной комнате
            var docs = await _context.Documents
                .Where(d =>
                    d.CreatedBy == userId ||
                    (d.RoomId != null && accessibleRoomIds.Contains(d.RoomId.Value)))
                .ToListAsync();

            // 3. Отбираем только последние версии
            var latestDocs = docs
                .GroupBy(d => d.Name) // или по логике группировки (например, OriginalId если есть)
                .Select(g => g.OrderByDescending(d => d.Version).First())
                .ToList();

            return latestDocs;
        }
        public async Task<IEnumerable<Document>> GetWithoutRoomAsync(Guid userId)
        {
            return await _context.Documents
                .Where(d => d.CreatedBy == userId && d.RoomId == null)
                .ToListAsync();
        }
        public async Task<IEnumerable<Document>> GetVersionChainAsync(Guid docId)
        {
            var root = await _context.Documents.FindAsync(docId);
            if (root == null)
                return Enumerable.Empty<Document>();

            var hash = root.Hash;
            return await _context.Documents
                .Where(d => d.Hash == hash || d.PrevHash == hash || d.ParentDocId == root.ParentDocId || d.Id == docId)
                .OrderBy(d => d.Version)
                .ToListAsync();
        }
        public async Task<DateTime?> GetLastChangeInRoomAsync(Guid roomId)
        {
            return await _context.Documents
                .Where(d => d.RoomId == roomId)
                .MaxAsync(d => (DateTime?)d.CreatedOn);
        }

        public async Task<IEnumerable<Document>> GetAllAsyncWithRoomAsync()
        {
            return await _context.Documents
                .Include(d => d.Room)
                .ToListAsync();
        }
        public async Task<IEnumerable<Document>> GetAccessibleToUserAsync(Guid userId)
        {
            var accessibleRoomIds = await _context.RoomUsers
                .Where(ru => ru.UserId == userId)
                .Select(ru => ru.RoomId)
                .ToListAsync();

            var documents = await _context.Documents
                .Where(doc =>
                    doc.CreatedBy == userId ||
                    (doc.RoomId != null && accessibleRoomIds.Contains(doc.RoomId.Value)) ||
                    doc.Tokens.Any(t => t.UserId == userId)
                )
                .Include(d => d.Room)
                .ToListAsync();

            return documents;
        }
        public async Task<IEnumerable<Document>> GetOtherAccessibleDocsAsync(Guid userId)
        {
            var accessibleDocs = await GetAccessibleToUserAsync(userId);

            var ownRoomIds = await _context.Rooms
                .Where(r => r.OwnerId == userId)
                .Select(r => r.Id)
                .ToListAsync();

            return accessibleDocs
                .Where(d => d.CreatedBy != userId &&
                           (d.RoomId == null || !ownRoomIds.Contains(d.RoomId.Value)))
                .ToList();
        }
        public async Task<IEnumerable<Document>> GetLatestVersionsForUserAsync(Guid userId)
        {
            var docs = await GetAccessibleToUserAsync(userId);

            var latest = docs
                .GroupBy(d => GetChainRootHash(d, docs))
                .Select(g => g.OrderByDescending(x => x.Version).First());

            return latest.ToList();
        }
        private static string GetChainRootHash(Document d, IEnumerable<Document> pool)
        {
            var current = d;
            while (true)
            {
                var parent = pool.FirstOrDefault(x => x.Id == current.ParentDocId);
                if (parent == null) return current.Hash;
                current = parent;
            }
        }
    }
}