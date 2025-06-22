using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
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
            var query = _context.Documents
                .Where(d => d.CreatedBy == userId)
                .Include(d => d.Room);
                    return await ExcludeArchived(query).ToListAsync();
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
            var accessibleRoomIds = await _context.RoomUsers
                .Where(ru => ru.UserId == userId)
                .Select(ru => ru.RoomId)
                .ToListAsync();

            var docs = await _context.Documents
                .Where(d =>
                    d.CreatedBy == userId ||
                    (d.RoomId != null && accessibleRoomIds.Contains(d.RoomId.Value)))
                .ToListAsync();

            var latestDocs = docs
                .GroupBy(d => d.Name)
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
            var root = await _context.Documents
        .AsNoTracking()
        .FirstOrDefaultAsync(d => d.Id == docId);
            
                while (root is { ParentDocId: not null })
                root = await _context.Documents
                                 .AsNoTracking()
                                 .FirstAsync(d => d.Id == root.ParentDocId);
            
                if (root == null)
                        return Enumerable.Empty<Document>();
            
                // ── 2. Собираем «свою» ветку — все документы, для которых
                //       root.Id входит в их восходящую цепочку ParentDocId  ────────────
                //       (реализовано обычной рекурсией в памяти)
            var versions = new List<Document> { root };
            var queue = new Queue<Guid>();
            queue.Enqueue(root.Id);
            
                // выбираем ВСЕ документы, чей ParentDocId не NULL
            var candidates = await _context.Documents
                    .AsNoTracking()
                    .Where(d => d.ParentDocId != null)
                    .ToListAsync();
            
                while (queue.Count > 0)
                    {
                var parentId = queue.Dequeue();
                
                        foreach (var child in candidates.Where(c => c.ParentDocId == parentId))
                            {
                    versions.Add(child);
                    queue.Enqueue(child.Id);
                            }
                    }
            
                return versions.OrderBy(d => d.CreatedOn);
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

            var query = _context.Documents
                .Where(doc =>
                    doc.CreatedBy == userId ||
                    (doc.RoomId != null && accessibleRoomIds.Contains(doc.RoomId.Value)) ||
                    doc.Tokens.Any(t => t.UserId == userId)
                )
                .Include(d => d.Room);

            return await ExcludeArchived(query).ToListAsync();
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
        public async Task<IEnumerable<Document>> GetArchivedForUserAsync(Guid userId)
        {
            return await _context.Documents
                .Where(d => d.Room != null &&
                            d.Room.Title == "Архив" &&
                            d.Room.OwnerId == userId)
                .ToListAsync();
        }
        private IQueryable<Document> ExcludeArchived(IQueryable<Document> query)
        {
            return query.Where(d => d.Room == null || d.Room.Title != "Архив");
        }

        public async Task<List<Document>> GetForkBranchAsync(Guid docId)
        {
            var doc = await _context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == docId);

            if (doc == null)
                return new List<Document>();

            return await _context.Documents
                .Where(d => d.ForkPath.StartsWith(doc.ForkPath))
                .ToListAsync();
        }
        public async Task<IEnumerable<Document>> GetLatestVersionsByForkPathAsync(Guid userId)
        {
            var all = await GetAccessibleToUserAsync(userId);

            var lastVersions = all
                .Where(doc => !all.Any(p => p.ParentDocId == doc.Id))
                .ToList();

            return lastVersions;
        }
    }
}