using Microsoft.EntityFrameworkCore;
using VlaDO.DTOs;
using VlaDO.Models;
using VlaDO.Repositories.Rooms;

namespace VlaDO.Repositories
{
    /// <summary>
    /// Специфичные операции с комнатами (приглашения, уровни доступа).
    /// Общие CRUD берутся из GenericRepository.
    /// </summary>
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        /// <summary>
        /// Список системных комнат, которые следует исключать из некоторых выборок.
        /// </summary>
        private static readonly HashSet<string> SystemRoomTitles = new(StringComparer.OrdinalIgnoreCase) {
        "Архив"
    };

        /// <summary>
        /// Создаёт новый экземпляр репозитория комнат.
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public RoomRepository(DocumentFlowContext context) : base(context) { }

        /// <summary>
        /// Приглашает пользователя в комнату с заданным уровнем доступа.
        /// </summary>
        /// <param name="roomId">ID комнаты</param>
        /// <param name="userId">ID пользователя</param>
        /// <param name="level">Уровень доступа</param>
        /// <returns>Результат</returns>
        public async Task AddUserToRoomAsync(Guid roomId, Guid userId, AccessLevel level)
        {
            var ru = new RoomUser { RoomId = roomId, UserId = userId, AccessLevel = level };
            await _context.RoomUsers.AddAsync(ru);
        }

        /// <summary>
        /// Удаляет пользователя из указанной комнаты.
        /// </summary>
        /// <param name="roomId">ID комнаты</param>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Результат</returns>
        public async Task RemoveUserFromRoomAsync(Guid roomId, Guid userId)
        {
            var ru = await _context.RoomUsers.FindAsync(roomId, userId);
            if (ru != null) _context.RoomUsers.Remove(ru);
        }

        /// <summary>
        /// Обновляет уровень доступа пользователя в указанной комнате.
        /// </summary>
        /// <param name="roomId">ID комнаты</param>
        /// <param name="userId">ID пользователя</param>
        /// <param name="newLevel">Новый уровень доступа</param>
        /// <returns>Результат</returns>
        public async Task UpdateUserAccessLevelAsync(Guid roomId, Guid userId, AccessLevel newLevel)
        {
            var ru = await _context.RoomUsers.FindAsync(roomId, userId);
            if (ru != null) ru.AccessLevel = newLevel;
        }

        /// <summary>
        /// Проверяет, является ли пользователь владельцем комнаты.
        /// </summary>
        /// <param name="roomId">ID комнаты</param>
        /// <param name="userId">ID пользователя</param>
        /// <returns>True, если пользователь владелец комнаты</returns>
        public Task<bool> IsRoomOwnerAsync(Guid roomId, Guid userId) =>
            _context.Rooms.AnyAsync(r => r.Id == roomId && r.OwnerId == userId);

        /// <summary>
        /// Возвращает все комнаты, созданные указанным пользователем.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Список комнат, где пользователь является владельцем</returns>
        public Task<IEnumerable<Room>> GetRoomsByOwnerAsync(Guid userId)
        {
            return _context.Rooms
                .Where(r => r.OwnerId == userId)
                .ToListAsync()
                .ContinueWith(t => (IEnumerable<Room>)t.Result);
        }

        /// <summary>
        /// Возвращает N последних комнат, в которых происходили изменения документов,
        /// и к которым пользователь имеет доступ.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="take">Максимальное количество результатов (по умолчанию 3)</param>
        /// <returns>Список краткой информации о комнатах</returns>
        public async Task<IEnumerable<RoomBriefDto>> GetRecentAsync(Guid userId, int take = 3)
        {
            var query =
                from d in _context.Documents
                where d.RoomId != null
                join ru in _context.RoomUsers
                     on new { RoomId = d.RoomId!.Value, userId }
                     equals new { ru.RoomId, userId }
                group d by d.RoomId into g
                let last = g.Max(x => x.CreatedOn)
                orderby last descending
                select new RoomBriefDto(
                    g.Key!.Value,
                    g.Select(x => x.Room!.Title)
                     .FirstOrDefault() ?? "(без названия)",
                    last);

            return await query.Take(take).ToListAsync();
        }

        /// <summary>
        /// Возвращает N последних комнат, в которых происходили изменения документов,
        /// и к которым пользователь имеет доступ.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="take">Максимальное количество результатов (по умолчанию 3)</param>
        /// <returns>Список краткой информации о комнатах</returns>
        public async Task<IEnumerable<RoomBriefDto>> SearchRoomsAsync(Guid userId, string? title = null, Guid? roomId = null, DateTime? since = null)
        {
            var query = _context.Rooms
                .Where(r => r.OwnerId == userId);

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(r => r.Title!.Contains(title));

            if (roomId.HasValue)
                query = query.Where(r => r.Id == roomId.Value);

            if (since.HasValue)
            {
                query = query.Where(r => _context.Documents
                    .Any(d => d.RoomId == r.Id && d.CreatedOn >= since.Value));
            }

            var result = await query.ToListAsync();

            return result.Select(r => new RoomBriefDto(
                r.Id,
                r.Title ?? "(без названия)",
                null
            ));
        }

        /// <summary>
        /// Получает список комнат, в которых пользователь участвует (не обязательно является владельцем).
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Список комнат с доступом</returns>
        public async Task<IEnumerable<Room>> GetByUserAsync(Guid userId)
        {
            return await _context.RoomUsers
                .Where(ru => ru.UserId == userId)
                .Select(ru => ru.Room)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// Проверяет, существует ли у пользователя комната с заданным названием (без учёта регистра).
        /// </summary>
        /// <param name="ownerId">ID владельца комнат</param>
        /// <param name="title">Название комнаты для проверки</param>
        /// <returns>true, если комната с таким названием уже существует, иначе false</returns>
        public async Task<bool> ExistsWithTitleAsync(Guid ownerId, string title)
        {
            var lowerTitle = title.ToLowerInvariant();

            var rooms = await _context.Rooms
                .Where(r => r.OwnerId == ownerId && r.Title != null)
                .Select(r => r.Title!)
                .ToListAsync();

            return rooms.Any(t => string.Equals(t, title, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Возвращает список всех пользовательских (не системных) комнат, созданных данным пользователем.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Список комнат с полным доступом</returns>
        public async Task<List<RoomWithAccessDto>> GetOwnedRoomsAsync(Guid userId)
        {
            return await _context.Rooms
                .Where(r => r.OwnerId == userId && !SystemRoomTitles.Contains(r.Title ?? ""))
                .Select(r => new RoomWithAccessDto(
                    r.Id,
                    r.Title ?? "(без названия)",
                    null,
                    "Full"
                ))
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает список всех комнат, в которых пользователь не является владельцем, но имеет доступ через приглашение.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Список комнат с указанием уровня доступа</returns>
        public async Task<List<RoomWithAccessDto>> GetForeignRoomsAsync(Guid userId)
        {
            var ownedRoomIds = await _context.Rooms
                .Where(r => r.OwnerId == userId)
                .Select(r => r.Id)
                .ToListAsync();

            return await _context.RoomUsers
                .Where(ru => ru.UserId == userId && !ownedRoomIds.Contains(ru.RoomId))
                .Select(ru => new RoomWithAccessDto(
                    ru.RoomId,
                    ru.Room.Title,
                    null,
                    ru.AccessLevel.ToString()
                ))
                .ToListAsync();
        }
    }
}