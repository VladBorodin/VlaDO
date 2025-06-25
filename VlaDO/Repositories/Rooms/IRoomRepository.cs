using VlaDO.DTOs;
using VlaDO.Models;

namespace VlaDO.Repositories.Rooms
{
    /// <summary>
    /// Интерфейс для работы с комнатами и правами доступа пользователей к ним.
    /// </summary>
    public interface IRoomRepository : IGenericRepository<Room>
    {
        /// <summary>
        /// Добавляет пользователя в комнату с указанным уровнем доступа.
        /// </summary>
        Task AddUserToRoomAsync(Guid roomId, Guid userId, AccessLevel level);

        /// <summary>
        /// Удаляет пользователя из комнаты.
        /// </summary>
        Task RemoveUserFromRoomAsync(Guid roomId, Guid userId);

        /// <summary>
        /// Обновляет уровень доступа пользователя к комнате.
        /// </summary>
        Task UpdateUserAccessLevelAsync(Guid roomId, Guid userId, AccessLevel newLevel);

        /// <summary>
        /// Проверяет, является ли пользователь владельцем комнаты.
        /// </summary>
        Task<bool> IsRoomOwnerAsync(Guid roomId, Guid userId);

        /// <summary>
        /// Возвращает список комнат, созданных пользователем.
        /// </summary>
        Task<IEnumerable<Room>> GetRoomsByOwnerAsync(Guid userId);

        /// <summary>
        /// Возвращает N последних комнат, где изменялись документы,
        /// и к которым пользователь имеет доступ.
        /// </summary>
        Task<IEnumerable<RoomBriefDto>> GetRecentAsync(Guid userId, int take = 3);

        /// <summary>
        /// Поиск комнат по названию, идентификатору или дате последнего изменения.
        /// </summary>
        Task<IEnumerable<RoomBriefDto>> SearchRoomsAsync(Guid userId, string? title = null, Guid? roomId = null, DateTime? since = null);

        /// <summary>
        /// Возвращает комнаты, к которым у пользователя есть доступ.
        /// </summary>
        Task<IEnumerable<Room>> GetByUserAsync(Guid userId);

        /// <summary>
        /// Проверяет, существует ли у пользователя комната с таким названием.
        /// </summary>
        Task<bool> ExistsWithTitleAsync(Guid ownerId, string title);

        /// <summary>
        /// Возвращает список собственных комнат пользователя с уровнями доступа.
        /// </summary>
        Task<List<RoomWithAccessDto>> GetOwnedRoomsAsync(Guid userId);

        /// <summary>
        /// Возвращает список чужих комнат, к которым у пользователя есть доступ.
        /// </summary>
        Task<List<RoomWithAccessDto>> GetForeignRoomsAsync(Guid userId);
    }
}
