using Microsoft.EntityFrameworkCore;
using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _uow;
    public PermissionService(IUnitOfWork u) => _uow = u;

    public async Task<bool> CheckAccessAsync(
    Guid userId, Guid docId, AccessLevel required, string? token = null)
    {
        var doc = await _uow.Documents.GetByIdAsync(docId, d => d.Room);
        if (doc == null) return false;

        if (doc.CreatedBy == userId)
            return true;

        if (doc.Room?.OwnerId == userId)
            return true;

        if (doc.RoomId is Guid roomId)
        {
            var ru = (await _uow.RoomUsers
                         .FindAsync(r => r.RoomId == roomId && r.UserId == userId))
                     .FirstOrDefault();

            if (ru != null && ru.AccessLevel >= required)
                return true;
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            var dt = (await _uow.Tokens
                .FindAsync(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow))
                .FirstOrDefault();

            if (dt != null && dt.AccessLevel >= required && dt.DocumentId == docId)
                return true;
        }

        return false;
    }

    public async Task<bool> CheckRoomAccessAsync(Guid userId, Guid roomId, AccessLevel level)
    {
        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room?.OwnerId == userId) return true;

        var ru = await _uow.RoomUsers
            .FirstOrDefaultAsync(r => r.RoomId == roomId && r.UserId == userId);

        return ru != null && ru.AccessLevel >= level;
    }
    public async Task<AccessLevel> GetAccessLevelAsync(Guid userId, Guid roomId)
    {
        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room?.OwnerId == userId)
            return AccessLevel.Full;

        var ru = await _uow.RoomUsers
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RoomId == roomId);

        return ru?.AccessLevel ?? AccessLevel.Read;
    }
    public async Task<List<Guid>> GetRoomsWithAccessAsync(Guid userId, AccessLevel minLevel)
    {
        var owned = await _uow.Rooms.FindAsync(r => r.OwnerId == userId);
        var invited = await _uow.RoomUsers
                               .FindAsync(ru => ru.UserId == userId && ru.AccessLevel >= minLevel);

        return owned.Select(r => r.Id)
                    .Concat(invited.Select(ru => ru.RoomId))
                    .Distinct()
                    .ToList();
    }

    public async Task<List<Guid>> GetDocsWithAccessAsync(Guid userId, AccessLevel minLevel)
    {
        var ownDocs = await _uow.Documents.FindAsync(d => d.CreatedBy == userId);

        var roomIds = await GetRoomsWithAccessAsync(userId, minLevel);
        var roomDocs = await _uow.Documents.FindAsync(d => d.RoomId != null &&
                                                           roomIds.Contains(d.RoomId!.Value));

        return ownDocs.Concat(roomDocs).Select(d => d.Id).Distinct().ToList();
    }
}
