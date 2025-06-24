using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services
{
    public class ActivityReadService : IActivityReadService
    {
        private readonly IGenericRepository<ActivityRead> _repo;
        private readonly IUnitOfWork _uow;

        public ActivityReadService(IGenericRepository<ActivityRead> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<bool> IsReadAsync(Guid activityId, Guid userId) =>
            await _repo.ExistsAsync(ar => ar.ActivityId == activityId && ar.UserId == userId);

        public async Task MarkAsReadAsync(Guid activityId, Guid userId)
        {
            if (await IsReadAsync(activityId, userId)) return;

            await _repo.AddAsync(new ActivityRead { ActivityId = activityId, UserId = userId });
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<Guid>> GetReadActivityIdsAsync(Guid userId)
        {
            var list = await _repo.FindAsync(ar => ar.UserId == userId);
            return list.Select(ar => ar.ActivityId);
        }
    }

}
