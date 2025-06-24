namespace VlaDO.Services
{
    public interface IActivityReadService
    {
        Task<bool> IsReadAsync(Guid activityId, Guid userId);
        Task MarkAsReadAsync(Guid activityId, Guid userId);
        Task<IEnumerable<Guid>> GetReadActivityIdsAsync(Guid userId);
    }
}
