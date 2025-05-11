using Microsoft.EntityFrameworkCore;
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
    }
}