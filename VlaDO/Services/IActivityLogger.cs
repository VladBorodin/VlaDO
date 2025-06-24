using VlaDO.Models;

namespace VlaDO.Services
{
    public interface IActivityLogger
    {
        Task LogAsync(ActivityType type, Guid authorId, Guid? subjectId = null, object? meta = null, Guid? toUserId = null);
    }
}
