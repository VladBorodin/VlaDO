using VlaDO.DTOs;
using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services;

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _uow;
    private readonly ActivityLogger _logger;
    public RoomService(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> CreateAsync(Guid ownerId, string? title)
    {
        if (!string.IsNullOrWhiteSpace(title) &&
        await _uow.Rooms.ExistsWithTitleAsync(ownerId, title))
            throw new InvalidOperationException("Комната с таким названием уже существует.");

        var room = new Room { OwnerId = ownerId, Title = title };
        await _uow.Rooms.AddAsync(room);
        await _uow.CommitAsync();
        await _logger.LogAsync(ActivityType.CreatedRoom, ownerId, room.Id, new { Title = title });
        return room.Id;
    }

    public async Task AddUserAsync(Guid roomId, Guid userId, AccessLevel level)
    {
        await _uow.Rooms.AddUserToRoomAsync(roomId, userId, level);
    }

    public async Task ChangeAccessAsync(Guid roomId, Guid userId, AccessLevel level)
    {
        await _uow.Rooms.UpdateUserAccessLevelAsync(roomId, userId, level);
    }

    public async Task RemoveUserAsync(Guid roomId, Guid userId)
    {
        await _uow.Rooms.RemoveUserFromRoomAsync(roomId, userId);
    }
    public Task<IEnumerable<RoomBriefDto>> GetRecentAsync(Guid userId, int take = 3)
        => _uow.Rooms.GetRecentAsync(userId, take);
    public async Task<Dictionary<string, List<RoomWithAccessDto>>> GetGroupedRoomsAsync(Guid userId)
    {
        var mine = await _uow.Rooms.GetOwnedRoomsAsync(userId);
        var others = await _uow.Rooms.GetForeignRoomsAsync(userId);

        return new Dictionary<string, List<RoomWithAccessDto>>
        {
            ["mine"] = mine,
            ["other"] = others
        };
    }
}