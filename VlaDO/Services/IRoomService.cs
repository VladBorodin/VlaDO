namespace VlaDO.Services;

using VlaDO.DTOs;
using VlaDO.Models;

public interface IRoomService
{
    Task<Guid> CreateAsync(Guid ownerId, string? title);
    Task AddUserAsync(Guid roomId, Guid userId, AccessLevel level);
    Task ChangeAccessAsync(Guid roomId, Guid userId, AccessLevel level);
    Task RemoveUserAsync(Guid roomId, Guid userId);
    Task<IEnumerable<RoomBriefDto>> GetRecentAsync(Guid userId, int take = 3);
    Task<Dictionary<string, List<RoomWithAccessDto>>> GetGroupedRoomsAsync(Guid userId);
}
