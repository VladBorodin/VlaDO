using VlaDO.Models;

namespace VlaDO.Repositories.Rooms
{
    public interface IRoomRepository : IGenericRepository<Room>
    {
        Task AddUserToRoomAsync(Guid roomId, Guid userId, AccessLevel level);
        Task RemoveUserFromRoomAsync(Guid roomId, Guid userId);
        Task UpdateUserAccessLevelAsync(Guid roomId, Guid userId, AccessLevel newLevel);
        Task<bool> IsRoomOwnerAsync(Guid roomId, Guid userId);
        Task<IEnumerable<Room>> GetRoomsByOwnerAsync(Guid userId);
    }
}
