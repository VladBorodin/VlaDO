using VlaDO.DTOs;
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
        /// <summary>
        /// N последних комнат, где изменялись документы
        /// и к которым <paramref name="userId"/> имеет доступ.
        /// </summary>
        Task<IEnumerable<RoomBriefDto>> GetRecentAsync(Guid userId, int take = 3);
        Task<IEnumerable<RoomBriefDto>> SearchRoomsAsync(Guid userId, string? title = null, Guid? roomId = null, DateTime? since = null);
        Task<IEnumerable<Room>> GetByUserAsync(Guid userId);
        Task<bool> ExistsWithTitleAsync(Guid ownerId, string title);
        Task<List<RoomWithAccessDto>> GetOwnedRoomsAsync(Guid userId);
        Task<List<RoomWithAccessDto>> GetForeignRoomsAsync(Guid userId);
    }
}
