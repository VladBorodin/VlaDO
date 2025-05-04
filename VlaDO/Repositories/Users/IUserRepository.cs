using VlaDO.Models;

namespace VlaDO.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<Room>> GetRoomsWithAccessAsync(Guid userId, AccessLevel level);
        Task<bool> IsRoomOwnerAsync(Guid roomId, Guid userId);
        Task<IEnumerable<Document>> GetAccessibleDocumentsAsync(Guid userId);
    }
}
