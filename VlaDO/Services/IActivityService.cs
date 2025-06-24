using VlaDO.Models;

namespace VlaDO.Services
{
    public interface IActivityService
    {
        Task LogAsync(ActivityType type, Guid? userId,
                      Guid? authorId = null,
                      Guid? roomId = null,
                      Guid? docId = null,
                      object? payload = null,
                      CancellationToken ct = default);
    }
}
