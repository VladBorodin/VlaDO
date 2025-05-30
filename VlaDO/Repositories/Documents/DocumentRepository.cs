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
    }
}