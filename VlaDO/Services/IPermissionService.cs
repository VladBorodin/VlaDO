using VlaDO.Models;

namespace VlaDO.Services;
public interface IPermissionService
{
    Task<bool> CheckAccessAsync(Guid userId, Guid docId, AccessLevel required, string? token = null);
}