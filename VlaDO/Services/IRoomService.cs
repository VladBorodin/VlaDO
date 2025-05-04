namespace VlaDO.Services;

using VlaDO.Models;

public interface IRoomService
{
    Task<Guid> CreateAsync(Guid ownerId, string? title);
    Task AddUserAsync(Guid roomId, Guid userId, AccessLevel level);
    Task ChangeAccessAsync(Guid roomId, Guid userId, AccessLevel level);
    Task RemoveUserAsync(Guid roomId, Guid userId);
}
