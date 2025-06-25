using VlaDO.DTOs;
using VlaDO.Models;

namespace VlaDO.Repositories
{
    /// <summary>
    /// Интерфейс для работы с пользователями, содержащий специфичные методы, выходящие за рамки стандартного CRUD.
    /// </summary>
    public interface IUserRepository : IGenericRepository<User>
    {
        /// <summary>
        /// Получает пользователя по адресу электронной почты.
        /// </summary>
        /// <param name="email">Электронная почта пользователя</param>
        /// <returns>Пользователь или null, если не найден</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Возвращает список комнат, к которым у пользователя есть доступ с заданным уровнем прав или выше.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="level">Минимальный уровень доступа</param>
        /// <returns>Список комнат</returns>
        Task<IEnumerable<Room>> GetRoomsWithAccessAsync(Guid userId, AccessLevel level);

        /// <summary>
        /// Проверяет, является ли пользователь владельцем указанной комнаты.
        /// </summary>
        /// <param name="roomId">ID комнаты</param>
        /// <param name="userId">ID пользователя</param>
        /// <returns>true, если пользователь является владельцем комнаты; иначе false</returns>
        Task<bool> IsRoomOwnerAsync(Guid roomId, Guid userId);

        /// <summary>
        /// Получает все документы, доступные пользователю (через комнаты или временные токены).
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Список документов</returns>
        Task<IEnumerable<Document>> GetAccessibleDocumentsAsync(Guid userId);

        /// <summary>
        /// Возвращает краткую информацию о пользователе по его ID.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>DTO с краткой информацией о пользователе или null, если не найден</returns>
        Task<UserBriefDto?> GetBriefByIdAsync(Guid userId);

        /// <summary>
        /// Получает пользователя по имени (без учета регистра и пробелов).
        /// </summary>
        /// <param name="name">Имя пользователя</param>
        /// <returns>Пользователь или null, если не найден</returns>
        Task<User?> GetByNameAsync(string name);
    }
}
