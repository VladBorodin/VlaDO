using VlaDO.Models;
using VlaDO.Repositories;
using System.Text.Json;

namespace VlaDO.Services
{
    public class ActivityLogger : IActivityLogger
    {
        private readonly IGenericRepository<Activity> _repo;
        private readonly IUnitOfWork _uow;

        public ActivityLogger(IGenericRepository<Activity> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task LogAsync(ActivityType type, Guid authorId, Guid? subjectId = null, object? meta = null, Guid? toUserId = null)
        {
            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Type = type,
                CreatedAt = DateTime.UtcNow,
                AuthorId = authorId,
                PayloadJson = meta != null ? JsonSerializer.Serialize(meta) : null
            };

            switch (type)
            {
                // Документ
                case ActivityType.CreatedDocument:
                case ActivityType.UpdatedDocument:
                case ActivityType.DeletedDocument:
                case ActivityType.ArchivedDocument:
                case ActivityType.RenamedDocument:
                case ActivityType.IssuedToken:
                case ActivityType.UpdatedToken:
                case ActivityType.RevokedToken:
                    activity.DocumentId = subjectId;
                    break;

                // Комната
                case ActivityType.CreatedRoom:
                case ActivityType.InvitedToRoom:
                case ActivityType.UpdatedRoomAccess:
                case ActivityType.DeletedRoom:
                    activity.RoomId = subjectId;
                    break;

                // Контакт
                case ActivityType.InvitedToContacts:
                case ActivityType.AcceptedContact:
                case ActivityType.DeclinedContact:
                    activity.UserId = subjectId;
                    break;
            }

            await _repo.AddAsync(activity);
            if (toUserId != null)
            {
                activity.UserId = toUserId;
                await _repo.AddAsync(activity);
            }
            await _uow.CommitAsync();
        }
    }
}
