using VlaDO.Models;

namespace VlaDO.Repositories.Documents
{
    public interface IDocumentTokenRepository : IGenericRepository<DocumentToken>
    {
        Task<DocumentToken?> GetByDocAndUserAsync(Guid documentId, Guid userId);
    }
}