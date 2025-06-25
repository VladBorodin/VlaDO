using VlaDO.Models;

namespace VlaDO.Services
{
    /// <summary>
    /// Сервис для записи активностей пользователя или системы.
    /// </summary>
    public interface IActivityService
    {
        /// <summary>
        /// Логирует активность с заданными параметрами.
        /// </summary>
        /// <param name="type">Тип активности (например, создание документа, вход в систему и т.п.).</param>
        /// <param name="userId">Идентификатор пользователя, для которого создаётся активность (адресат).</param>
        /// <param name="authorId">Идентификатор автора активности (может отличаться от <paramref name="userId"/>, если кто-то совершает действие для другого).</param>
        /// <param name="roomId">Идентификатор комнаты, если активность связана с ней.</param>
        /// <param name="docId">Идентификатор документа, если активность связана с ним.</param>
        /// <param name="payload">Дополнительные метаданные или данные, связанные с активностью.</param>
        /// <param name="ct">Токен отмены для асинхронной операции.</param>
        Task LogAsync(
            ActivityType type,
            Guid? userId,
            Guid? authorId = null,
            Guid? roomId = null,
            Guid? docId = null,
            object? payload = null,
            CancellationToken ct = default);
    }
}