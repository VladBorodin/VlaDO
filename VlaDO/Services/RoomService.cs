using VlaDO.DTOs;
using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services;

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _uow;
    public RoomService(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> CreateAsync(Guid ownerId, string? title)
    {
        var room = new Room { OwnerId = ownerId, Title = title };
        await _uow.Rooms.AddAsync(room);
        await _uow.CommitAsync();
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
}