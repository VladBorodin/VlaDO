using VlaDO.Models;

namespace VlaDO.Services
{
    public interface IDocumentService
    {
        Task<Guid> UploadAsync(Guid roomId, Guid userId, string name, byte[] data);
        Task<IEnumerable<Document>> ListAsync(Guid roomId);
        Task<Document?> GetAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
