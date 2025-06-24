using System.Text.Json;
using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services
{

    public class ActivityService : IActivityService
    {
        private readonly IUnitOfWork _uow;
        public ActivityService(IUnitOfWork uow) => _uow = uow;

        public async Task LogAsync(
            ActivityType type,
            Guid? userId,
            Guid? authorId = null,
            Guid? roomId = null,
            Guid? docId = null,
            object? payload = null,
            CancellationToken ct = default)
        {
            var act = new Activity
            {
                Id = Guid.NewGuid(),
                Type = type,
                UserId = userId,
                AuthorId = authorId,
                RoomId = roomId,
                DocumentId = docId,
                PayloadJson = payload == null ? null : JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Activities.AddAsync(act);
            await _uow.CommitAsync();
        }
    }
}