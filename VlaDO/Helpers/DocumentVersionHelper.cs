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
            return (parent.Version + 1, parent.ForkPath);

        var parentDepth = parent.ForkPath.Count(ch => ch == '.');
        var prefix = parentDepth == 0 ? "" : parent.ForkPath.Substring(0, parent.ForkPath.LastIndexOf('.') + 1);

        if (children.Any(c => c.ForkPath == parent.ForkPath))
        {
            var sameDepthNums = children
                .Where(c => c.ForkPath.Count(ch => ch == '.') == parentDepth)
                .Select(c => ExtractLastForkNumber(c.ForkPath))
                .ToList();

            sameDepthNums.Add(ExtractLastForkNumber(parent.ForkPath));

            var nextNum = sameDepthNums.Max() + 1;
            var newPath = $"{prefix}{nextNum}";
            return (parent.Version + 1, newPath);
        }

        var deeperChildren = children
            .Where(c => c.ForkPath.StartsWith(parent.ForkPath + "."))
            .Select(c => ExtractLastForkNumber(c.ForkPath));

        if (deeperChildren.Any())
        {
            var next = deeperChildren.Max() + 1;
            return (parent.Version + 1, $"{parent.ForkPath}.{next}");
        }

        return (parent.Version + 1, parent.ForkPath);
    }

    /// <summary>
    /// Извлекает последнюю часть пути вилки (после последней точки)
    /// </summary>
    private static int ExtractLastForkNumber(string forkPath)
    {
        var parts = forkPath.Split('.');
        return int.TryParse(parts.Last(), out var num) ? num : 0;
    }

    public static async Task<string> GenerateInitialForkPathAsync(IGenericRepository<Document> repo, Guid userId, Guid roomId)
    {
        var rootForks = await repo.FindAsync(d =>
            (d.RoomId ?? Guid.Empty) == roomId &&
            d.ParentDocId == null &&
            d.ForkPath != null &&
            !d.ForkPath.Contains("-"));

        Console.WriteLine($"[ForkGen] Поиск корней room={roomId}");
        foreach (var doc in rootForks)
        {
            Console.WriteLine($"  ├─ Документ: ForkPath={doc.ForkPath}, RoomId={doc.RoomId}");
        }

        var maxRoot = rootForks
            .Select(d => int.TryParse(d.ForkPath, out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();

        Console.WriteLine($"[ForkGen] Max ForkPath: {maxRoot}, возвращаю: {maxRoot + 1}");

        return $"{maxRoot + 1}";
    }
    public static async Task<string> SafeGenerateInitialForkPathAsync(
        IGenericRepository<Document> repo, Guid userId, Guid roomId, int maxAttempts = 5)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            var forkPath = await GenerateInitialForkPathAsync(repo, userId, roomId);

            var exists = await repo.AnyAsync(d =>
                d.CreatedBy == userId &&
                (d.RoomId ?? Guid.Empty) == roomId &&
                d.ParentDocId == null &&
                d.ForkPath == forkPath);

            if (!exists)
                return forkPath;

            await Task.Delay(30); // для избежания гонки
        }

        throw new InvalidOperationException("Не удалось сгенерировать уникальный ForkPath после повторов.");
    }

}
