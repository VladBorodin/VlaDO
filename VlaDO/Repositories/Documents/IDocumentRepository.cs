using VlaDO.Models;

namespace VlaDO.Repositories.Documents
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        Task<IEnumerable<Document>> GetByCreatorAsync(Guid userId);
        Task<IEnumerable<Document>> GetByRoomAsync(Guid roomId);
        Task<IEnumerable<Document>> GetByRoomAndUserAsync(Guid roomId, Guid userId);
        Task<IEnumerable<Document>> GetByRoomAndUserAsyncExcludeCreator(Guid userId);
        Task<IEnumerable<Document>> GetWithoutRoomAsync(Guid userId);
        Task<IEnumerable<Document>> GetVersionChainAsync(Guid docId);
        Task<DateTime?> GetLastChangeInRoomAsync(Guid roomId);
        Task<IEnumerable<Document>> GetAccessibleToUserAsync(Guid userId);
        Task<IEnumerable<Document>> GetLatestVersionsForUserAsync(Guid userId);
        Task<IEnumerable<Document>> GetOtherAccessibleDocsAsync(Guid userId);
    }
}
