using VlaDO.Models;

namespace VlaDO.Services;
public interface IPermissionService
{
    Task<bool> CheckAccessAsync(Guid userId, Guid roomId, AccessLevel required, string? token = null);
    Task<bool> CheckRoomAccessAsync(Guid userId, Guid roomId, AccessLevel level);
    Task<AccessLevel> GetAccessLevelAsync(Guid userId, Guid roomId);
    Task<List<Guid>> GetRoomsWithAccessAsync(Guid userId, AccessLevel minLevel);
    Task<List<Guid>> GetDocsWithAccessAsync(Guid userId, AccessLevel minLevel);
}