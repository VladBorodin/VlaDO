using Microsoft.EntityFrameworkCore;
using VlaDO.DTOs;
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
    public async Task<IEnumerable<RoomBriefDto>> GetRecentAsync(Guid userId, int take = 3)
    {
        var query =
            from d in _context.Documents
            where d.RoomId != null
            join ru in _context.RoomUsers
                 on new { RoomId = d.RoomId!.Value, userId }
                 equals new { ru.RoomId, userId }
            group d by d.RoomId into g
            let last = g.Max(x => x.CreatedOn)
            orderby last descending
            select new RoomBriefDto(
                g.Key!.Value,
                g.Select(x => x.Room!.Title)
                 .FirstOrDefault() ?? "(без названия)",
                last);

        return await query.Take(take).ToListAsync();
    }
    public async Task<IEnumerable<RoomBriefDto>> SearchRoomsAsync(Guid userId, string? title = null, Guid? roomId = null, DateTime? since = null)
    {
        var query = _context.Rooms
            .Where(r => r.OwnerId == userId);

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(r => r.Title!.Contains(title));

        if (roomId.HasValue)
            query = query.Where(r => r.Id == roomId.Value);

        if (since.HasValue)
        {
            query = query.Where(r => _context.Documents
                .Any(d => d.RoomId == r.Id && d.CreatedOn >= since.Value));
        }

        var result = await query.ToListAsync();

        return result.Select(r => new RoomBriefDto(
            r.Id,
            r.Title ?? "(без названия)",
            null
        ));
    }
    public async Task<IEnumerable<Room>> GetByUserAsync(Guid userId)
    {
        return await _context.RoomUsers
            .Where(ru => ru.UserId == userId)
            .Select(ru => ru.Room)
            .Distinct()
            .ToListAsync();
    }

}
