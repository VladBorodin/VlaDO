using VlaDO.DTOs;
using VlaDO.Models;

namespace VlaDO.Services
{
    public interface IDocumentService
    {
        Task<Guid> UploadAsync(Guid roomId, Guid userId, string name, byte[] data);
        Task UploadManyAsync(Guid roomId, Guid userId, IEnumerable<IFormFile> files);
        Task<Guid> UpdateAsync(Guid roomId, Guid docId, Guid userId, IFormFile newFile, string? note = null);
        Task<(byte[] bytes, string fileName, string ctype)> DownloadAsync(Guid docId, Guid userId);
        Task<(byte[] zip, string fileName)> DownloadManyAsync(IEnumerable<Guid> ids, Guid userId);
        Task<IEnumerable<DocumentInfoDto>> ListAsync(Guid roomId, Guid userId);
        Task DeleteAsync(Guid docId, Guid userId); 
        Task<IEnumerable<Document>> GetByRoomAndUserAsyncExcludeCreator(Guid userId);
    }
}
