using Microsoft.EntityFrameworkCore;
using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _uow;
    public PermissionService(IUnitOfWork u) => _uow = u;

    // ────────────────────────────────────────── ДОКУМЕНТ
    public async Task<bool> CheckAccessAsync(
    Guid userId, Guid docId, AccessLevel required, string? token = null)
    {
        var doc = await _uow.Documents.GetByIdAsync(docId, d => d.Room);
        if (doc == null) return false;

        // ① Создатель документа
        if (doc.CreatedBy == userId)
            return true;

        // ② Владелец комнаты
        if (doc.Room?.OwnerId == userId)
            return true;

        // ③ Участник комнаты
        if (doc.RoomId is Guid roomId)
        {
            var ru = (await _uow.RoomUsers
                         .FindAsync(r => r.RoomId == roomId && r.UserId == userId))
                     .FirstOrDefault();

            if (ru != null && ru.AccessLevel >= required)
                return true;
        }

        // ④ Токен доступа
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

    // ────────────────────────────────────────── КОМНАТА
    public async Task<bool> CheckRoomAccessAsync(Guid userId, Guid roomId, AccessLevel level)
    {
        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room?.OwnerId == userId) return true;

        var ru = (await _uow.RoomUsers
                     .FindAsync(r => r.RoomId == roomId && r.UserId == userId))
                 .FirstOrDefault();                       // ← обычный LINQ

        return ru != null && ru.AccessLevel >= level;
    }
    public async Task<AccessLevel> GetAccessLevelAsync(Guid userId, Guid roomId)
    {
        var roomUser = await _uow.RoomUsers.FirstOrDefaultAsync(
            ru => ru.UserId == userId && ru.RoomId == roomId);

        return roomUser?.AccessLevel ?? AccessLevel.Read;
    }
}
