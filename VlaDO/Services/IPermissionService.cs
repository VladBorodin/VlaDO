using VlaDO.Models;

namespace VlaDO.Services;

/// <summary>
/// Сервис для проверки прав доступа пользователей к комнатам и документам.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Проверяет, имеет ли пользователь доступ к комнате или документу с учетом токена.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="roomId">Идентификатор комнаты.</param>
    /// <param name="required">Требуемый уровень доступа.</param>
    /// <param name="token">Опциональный токен доступа (например, для временных ссылок).</param>
    /// <returns>True, если доступ разрешен; иначе — false.</returns>
    Task<bool> CheckAccessAsync(Guid userId, Guid roomId, AccessLevel required, string? token = null);

    /// <summary>
    /// Проверяет, имеет ли пользователь указанный уровень доступа к комнате.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="roomId">Идентификатор комнаты.</param>
    /// <param name="level">Уровень доступа, который требуется проверить.</param>
    /// <returns>True, если доступ разрешен; иначе — false.</returns>
    Task<bool> CheckRoomAccessAsync(Guid userId, Guid roomId, AccessLevel level);

    /// <summary>
    /// Получает текущий уровень доступа пользователя к комнате.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="roomId">Идентификатор комнаты.</param>
    /// <returns>Фактический уровень доступа.</returns>
    Task<AccessLevel> GetAccessLevelAsync(Guid userId, Guid roomId);

    /// <summary>
    /// Получает список комнат, в которых у пользователя есть доступ не ниже заданного уровня.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="minLevel">Минимальный уровень доступа.</param>
    /// <returns>Список идентификаторов комнат.</returns>
    Task<List<Guid>> GetRoomsWithAccessAsync(Guid userId, AccessLevel minLevel);

    /// <summary>
    /// Получает список документов, доступных пользователю на основе заданного минимального уровня доступа.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="minLevel">Минимальный уровень доступа.</param>
    /// <returns>Список идентификаторов документов.</returns>
    Task<List<Guid>> GetDocsWithAccessAsync(Guid userId, AccessLevel minLevel);
}
