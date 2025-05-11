using VlaDO.Models;

namespace VlaDO.Repositories.Documents
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        Task<IEnumerable<Document>> GetByCreatorAsync(Guid userId);
        Task<IEnumerable<Document>> GetByRoomAsync(Guid roomId);
        Task<IEnumerable<Document>> GetByRoomAndUserAsync(Guid roomId, Guid userId);
    }
}
