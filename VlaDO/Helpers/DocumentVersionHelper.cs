using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Helpers
{

    /// <summary>
    /// Вспомогательные методы для генерации версий и ForkPath документов.
    /// </summary>
    public static class DocumentVersionHelper
    {
        /// <summary>
        /// Генерирует следующую версию и путь форка (ForkPath) для документа на основе его потомков.
        /// </summary>
        /// <param name="docRepo">Репозиторий документов.</param>
        /// <param name="parent">Родительский документ, для которого создается новая версия.</param>
        /// <returns>Кортеж из следующей версии и нового ForkPath.</returns>
        public static async Task<(int version, string forkPath)> GenerateNextVersionAsync(
            IGenericRepository<Document> repo, Document parent)
        {
            var children = await repo.FindAsync(d => d.ParentDocId == parent.Id);

            if (!children.Any())
                return (parent.Version + 1, parent.ForkPath);

            int parentDepth = parent.ForkPath.Count(ch => ch == '.');
            bool hasContinuation = children.Any(c => c.ForkPath == parent.ForkPath);

            if (hasContinuation)
            {
                if (parentDepth == 0)
                {
                    var rootSiblings = children
                        .Where(c => !c.ForkPath.Contains('.'))
                        .Select(c => ExtractLastForkNumber(c.ForkPath))
                        .Append(ExtractLastForkNumber(parent.ForkPath));

                    var next = rootSiblings.Max() + 1;
                    return (parent.Version + 1, $"{next}");
                }
                else
                {
                    var immediateForks = children
                        .Where(c => c.ForkPath.StartsWith(parent.ForkPath + ".") &&
                                    c.ForkPath.Count(ch => ch == '.') == parentDepth + 1)
                        .Select(c => ExtractLastForkNumber(c.ForkPath));

                    var next = immediateForks.DefaultIfEmpty(0).Max() + 1;
                    return (parent.Version + 1, $"{parent.ForkPath}.{next}");
                }
            }

            var deeper = children
                .Where(c => c.ForkPath.StartsWith(parent.ForkPath + "."))
                .Select(c => ExtractLastForkNumber(c.ForkPath));

            if (deeper.Any())
            {
                var next = deeper.Max() + 1;
                return (parent.Version + 1, $"{parent.ForkPath}.{next}");
            }

            return (parent.Version + 1, parent.ForkPath);
        }


        /// <summary>
        /// Извлекает последнюю числовую часть из ForkPath (после последней точки).
        /// </summary>
        /// <param name="forkPath">Строка ForkPath, например "1.3.2".</param>
        /// <returns>Последнее число в ForkPath или 0, если не удалось распарсить.</returns>
        private static int ExtractLastForkNumber(string forkPath)
        {
            var parts = forkPath.Split('.');
            return int.TryParse(parts.Last(), out var num) ? num : 0;
        }

        /// <summary>
        /// Генерирует начальный ForkPath (без родителя) для нового документа в комнате.
        /// Используется, когда документ не имеет родителя и должен получить уникальный корень.
        /// </summary>
        /// <param name="repo">Репозиторий документов.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <param name="roomId">Идентификатор комнаты.</param>
        /// <returns>ForkPath в формате целого числа, например "3".</returns>
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

        /// <summary>
        /// Надёжно генерирует уникальный начальный ForkPath, повторяя попытки при конфликте.
        /// </summary>
        /// <param name="repo">Репозиторий документов.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <param name="roomId">Идентификатор комнаты.</param>
        /// <param name="maxAttempts">Максимальное число попыток при коллизии.</param>
        /// <returns>Уникальный ForkPath.</returns>
        /// <exception cref="InvalidOperationException">Если не удалось сгенерировать уникальный путь после повторов.</exception>
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
}