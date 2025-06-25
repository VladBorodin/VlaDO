namespace VlaDO.Services;

using VlaDO.DTOs;
using VlaDO.Models;

/// <summary>
/// Сервис управления комнатами и доступом к ним.
/// </summary>
public interface IRoomService
{
    /// <summary>
    /// Создаёт новую комнату.
    /// </summary>
    /// <param name="ownerId">Идентификатор владельца комнаты.</param>
    /// <param name="title">Название комнаты (опционально).</param>
    /// <returns>Идентификатор созданной комнаты.</returns>
    Task<Guid> CreateAsync(Guid ownerId, string? title);

    /// <summary>
    /// Добавляет пользователя в комнату с указанным уровнем доступа.
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="level">Уровень доступа.</param>
    Task AddUserAsync(Guid roomId, Guid userId, AccessLevel level);

    /// <summary>
    /// Изменяет уровень доступа пользователя к комнате.
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="level">Новый уровень доступа.</param>
    Task ChangeAccessAsync(Guid roomId, Guid userId, AccessLevel level);

    /// <summary>
    /// Удаляет пользователя из комнаты.
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    Task RemoveUserAsync(Guid roomId, Guid userId);

    /// <summary>
    /// Возвращает список последних комнат, с которыми взаимодействовал пользователь.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="take">Количество комнат для выборки (по умолчанию 3).</param>
    /// <returns>Список краткой информации о комнатах.</returns>
    Task<IEnumerable<RoomBriefDto>> GetRecentAsync(Guid userId, int take = 3);

    /// <summary>
    /// Возвращает сгруппированные комнаты по признаку: созданы пользователем или нет.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Словарь, где ключ — название группы ("Мои", "Другие"), а значение — список комнат с уровнем доступа.</returns>
    Task<Dictionary<string, List<RoomWithAccessDto>>> GetGroupedRoomsAsync(Guid userId);
}
