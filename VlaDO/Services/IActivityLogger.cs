using VlaDO.Models;

namespace VlaDO.Services
{
    public interface IActivityLogger
    {
        Task LogAsync(ActivityType type, Guid authorId, Guid? subjectId = null, object? meta = null, Guid? toUserId = null);
        Task LogForUsersAsync(ActivityType type, IEnumerable<Guid> userIds, Guid authorId, Guid? roomId = null, Guid? docId = null, object? meta = null);
    }
}
