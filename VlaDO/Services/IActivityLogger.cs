using VlaDO.Models;

namespace VlaDO.Services
{
    /// <summary>
    /// Сервис для записи пользовательской активности и уведомлений.
    /// </summary>
    public interface IActivityLogger
    {
        /// <summary>
        /// Записывает одну активность в систему.
        /// </summary>
        /// <param name="type">Тип действия (например, создание документа, приглашение в комнату и т.д.).</param>
        /// <param name="authorId">Идентификатор пользователя, совершившего действие.</param>
        /// <param name="subjectId">Необязательный идентификатор субъекта действия (например, документа или комнаты).</param>
        /// <param name="meta">Дополнительные данные, связанные с действием.</param>
        /// <param name="toUserId">Необязательный идентификатор пользователя, для которого предназначено уведомление.</param>
        Task LogAsync(
            ActivityType type,
            Guid authorId,
            Guid? subjectId = null,
            object? meta = null,
            Guid? toUserId = null);

        /// <summary>
        /// Записывает одно действие для нескольких пользователей (например, уведомление о приглашении).
        /// </summary>
        /// <param name="type">Тип действия.</param>
        /// <param name="userIds">Список пользователей, которым адресовано действие.</param>
        /// <param name="authorId">Идентификатор автора действия.</param>
        /// <param name="roomId">Необязательный идентификатор комнаты, связанной с действием.</param>
        /// <param name="docId">Необязательный идентификатор документа, связанного с действием.</param>
        /// <param name="meta">Дополнительные данные о действии.</param>
        Task LogForUsersAsync(
            ActivityType type,
            IEnumerable<Guid> userIds,
            Guid authorId,
            Guid? roomId = null,
            Guid? docId = null,
            object? meta = null);
    }
}
