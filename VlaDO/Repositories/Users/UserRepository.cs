using Microsoft.EntityFrameworkCore;
using VlaDO.DTOs;
using VlaDO.Models;

namespace VlaDO.Repositories
{
    /// <summary>
    /// Репозиторий для работы с пользователями, включает специфичные методы, помимо CRUD.
    /// </summary>
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="UserRepository"/>.
        /// </summary>
        /// <param name="ctx">Контекст базы данных</param>
        public UserRepository(DocumentFlowContext ctx) : base(ctx) { }

        /// <summary>
        /// Получает пользователя по email.
        /// </summary>
        /// <param name="email">Электронная почта пользователя</param>
        /// <returns>Пользователь или null, если не найден</returns>
        public Task<User?> GetByEmailAsync(string email) =>
            _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        /// <summary>
        /// Возвращает список комнат, к которым у пользователя есть доступ с указанным или более высоким уровнем доступа.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="level">Минимальный уровень доступа</param>
        /// <returns>Список комнат</returns>
        public async Task<IEnumerable<Room>> GetRoomsWithAccessAsync(Guid userId, AccessLevel level)
        {
            return await _context.RoomUsers
                .Where(ru => ru.UserId == userId && ru.AccessLevel >= level)
                .Include(ru => ru.Room)
                .Select(ru => ru.Room)
                .ToListAsync();
        }

        /// <summary>
        /// Проверяет, является ли пользователь владельцем указанной комнаты.
        /// </summary>
        /// <param name="roomId">ID комнаты</param>
        /// <param name="userId">ID пользователя</param>
        /// <returns>true, если является владельцем; иначе false</returns>
        public Task<bool> IsRoomOwnerAsync(Guid roomId, Guid userId) =>
            _context.Rooms.AnyAsync(r => r.Id == roomId && r.OwnerId == userId);

        /// <summary>
        /// Получает все документы, доступные пользователю через комнаты или временные токены.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Список доступных документов</returns>
        public async Task<IEnumerable<Document>> GetAccessibleDocumentsAsync(Guid userId)
        {
            var roomDocs = await _context.RoomUsers
                .Where(ru => ru.UserId == userId && ru.AccessLevel >= AccessLevel.Read)
                .SelectMany(ru => ru.Room.Documents)
                .ToListAsync();

            var tokenDocs = await _context.DocumentTokens
                .Where(dt => dt.ExpiresAt > DateTime.UtcNow &&
                             dt.AccessLevel >= AccessLevel.Read &&
                             dt.Document.CreatedBy == userId)
                .Select(dt => dt.Document)
                .ToListAsync();

            return roomDocs.Union(tokenDocs).Distinct();
        }

        /// <summary>
        /// Возвращает краткую информацию о пользователе по его ID.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>DTO с краткой информацией или null, если пользователь не найден</returns>
        public async Task<UserBriefDto?> GetBriefByIdAsync(Guid userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserBriefDto(u.Id, u.Name))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Получает пользователя по имени (без учёта регистра и пробелов по краям).
        /// </summary>
        /// <param name="name">Имя пользователя</param>
        /// <returns>Пользователь или null, если не найден</returns>
        public Task<User?> GetByNameAsync(string name) =>
            _context.Users.FirstOrDefaultAsync(u =>
                u.Name.ToLower() == name.Trim().ToLower());
    }
}