using Microsoft.EntityFrameworkCore;
using VlaDO.Models;
using VlaDO.Repositories.Rooms;

namespace VlaDO.Repositories;

/// <summary>
/// Специфичные операции с комнатами (приглашения, уровни доступа).
/// Общие CRUD берутся из GenericRepository.
/// </summary>
public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(DocumentFlowContext context) : base(context) { }

    public async Task AddUserToRoomAsync(Guid roomId, Guid userId, AccessLevel level)
    {
        var ru = new RoomUser { RoomId = roomId, UserId = userId, AccessLevel = level };
        await _context.RoomUsers.AddAsync(ru);
    }

    public async Task RemoveUserFromRoomAsync(Guid roomId, Guid userId)
    {
        var ru = await _context.RoomUsers.FindAsync(roomId, userId);
        if (ru != null) _context.RoomUsers.Remove(ru);
    }

    public async Task UpdateUserAccessLevelAsync(Guid roomId, Guid userId, AccessLevel newLevel)
    {
        var ru = await _context.RoomUsers.FindAsync(roomId, userId);
        if (ru != null) ru.AccessLevel = newLevel;
    }

    public Task<bool> IsRoomOwnerAsync(Guid roomId, Guid userId) =>
        _context.Rooms.AnyAsync(r => r.Id == roomId && r.OwnerId == userId);

    public Task<IEnumerable<Room>> GetRoomsByOwnerAsync(Guid userId)
    {
        return _context.Rooms
            .Where(r => r.OwnerId == userId)
            .ToListAsync()
            .ContinueWith(t => (IEnumerable<Room>)t.Result);
    }
}
