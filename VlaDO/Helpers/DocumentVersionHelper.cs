using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Helpers;

public static class DocumentVersionHelper
{
    /// <summary>
    /// Генерация ForkPath для новой версии
    /// </summary>
    public static async Task<(int version, string forkPath)> GenerateNextVersionAsync(
        IGenericRepository<Document> docRepo,
        Document parent)
    {
        var children = await docRepo.FindAsync(d => d.ParentDocId == parent.Id);

        if (!children.Any())
        {
            return (parent.Version + 1, parent.ForkPath);
        }

        var forkChildren = children
            .Where(c => c.ForkPath.StartsWith(parent.ForkPath + "."))
            .Select(c => c.ForkPath)
            .ToList();

        var maxFork = forkChildren
            .Select(fp => ExtractLastForkNumber(fp))
            .DefaultIfEmpty(0)
            .Max();

        var newForkPath = $"{parent.ForkPath}.{maxFork + 1}";

        return (parent.Version + 1, newForkPath);
    }

    /// <summary>
    /// Извлекает последнюю часть пути вилки (после последней точки)
    /// </summary>
    private static int ExtractLastForkNumber(string forkPath)
    {
        var parts = forkPath.Split('.');
        return int.TryParse(parts.Last(), out var num) ? num : 0;
    }

    public static async Task<string> GenerateInitialForkPathAsync(IGenericRepository<Document> repo, Guid userId)
    {
        var rootForks = await repo.FindAsync(d => d.CreatedBy == userId && d.ParentDocId == null);

        var maxRoot = rootForks
            .Select(d =>
            {
                var firstSegment = d.ForkPath.Split('.')[0];
                return int.TryParse(firstSegment, out var num) ? num : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return (maxRoot + 1).ToString();
    }
}
