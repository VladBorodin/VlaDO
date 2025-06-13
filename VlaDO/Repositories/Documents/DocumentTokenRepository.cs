using Microsoft.EntityFrameworkCore;
using VlaDO.Models;

namespace VlaDO.Repositories.Documents
{
    public class DocumentTokenRepository : GenericRepository<DocumentToken>, IDocumentTokenRepository
    {
        public DocumentTokenRepository(DocumentFlowContext context) : base(context)
        {
        }

        public async Task<DocumentToken?> GetByDocAndUserAsync(Guid documentId, Guid userId)
        {
            return await _context.DocumentTokens
                .FirstOrDefaultAsync(dt => dt.DocumentId == documentId && dt.UserId == userId);
        }
    }
}
