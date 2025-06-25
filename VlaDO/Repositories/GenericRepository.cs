using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VlaDO.Models;

namespace VlaDO.Repositories
{
    /// <summary>
    /// Универсальный репозиторий, реализующий базовые CRUD-операции для любого класса сущности.
    /// </summary>
    /// <typeparam name="T">Тип сущности, с которой работает репозиторий</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Контекст базы данных, используемый для доступа к данным.
        /// </summary>
        protected readonly DocumentFlowContext _context;
        
        /// <summary>
        /// Набор данных (таблица), связанный с сущностью типа <typeparamref name="T"/>.
        /// </summary>
        private readonly DbSet<T> _set;

        /// <summary>
        /// Инициализирует новый экземпляр репозитория с указанным контекстом базы данных.
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public GenericRepository(DocumentFlowContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        /// <summary>
        /// Возвращает все сущности данного типа из базы данных.
        /// </summary>
        /// <returns>Список всех сущностей типа <typeparamref name="T"/></returns>
        public async Task<IEnumerable<T>> GetAllAsync() => (IEnumerable<T>)await _set.ToListAsync();

        /// <summary>
        /// Ищет сущность по идентификатору и включает связанные данные.
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="inc">Коллекция выражений для включения связанных данных (Include)</param>
        /// <returns>Найденная сущность или null, если не найдена</returns>
        public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] inc)
        {
            IQueryable<T> q = _set;
            q = inc.Aggregate(q, (cur, i) => cur.Include(i));
            return await q.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        /// <summary>
        /// Возвращает постраничный список сущностей.
        /// </summary>
        /// <param name="page">Номер страницы (нумерация начинается с 1)</param>
        /// <param name="size">Количество элементов на странице</param>
        /// <returns>Список сущностей на указанной странице</returns>
        public async Task<IEnumerable<T>> GetPagedAsync(int page, int size) => (IEnumerable<T>)await _set
           .Skip((page - 1) * size)
           .Take(size)
           .ToListAsync();

        /// <summary>
        /// Выполняет поиск сущностей по заданному предикату, с возможностью сортировки и включения навигационных свойств.
        /// </summary>
        /// <param name="pred">Предикат для фильтрации сущностей</param>
        /// <param name="orderBy">Опционально: функция для сортировки результата</param>
        /// <param name="include">Опционально: выражения для включения связанных сущностей (Include)</param>
        /// <returns>Список найденных сущностей</returns>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> pred, Func<IQueryable<T>,
            IOrderedQueryable<T>>? orderBy = null, params Expression<Func<T, object>>[] include)
        {
            IQueryable<T> q = _set;
            q = include.Aggregate(q, (cur, i) => cur.Include(i));
            q = q.Where(pred);
            if (orderBy != null) q = orderBy(q);
            return await q.ToListAsync();
        }

        /// <summary>
        /// Проверяет наличие сущности с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>True, если сущность существует; иначе — false</returns>
        public Task<bool> ExistsAsync(Guid id) =>
            _set.AnyAsync(e => EF.Property<Guid>(e, "Id") == id);

        /// <summary>
        /// Добавляет одну сущность в контекст.
        /// </summary>
        /// <param name="e">Сущность для добавления</param>
        /// <returns>Задача, представляющая завершение операции</returns>
        public Task AddAsync(T e) { _set.Add(e); return Task.CompletedTask; }

        /// <summary>
        /// Добавляет несколько сущностей в контекст.
        /// </summary>
        /// <param name="en">Коллекция сущностей для добавления</param>
        /// <returns>Задача, представляющая завершение операции</returns>
        public Task AddRangeAsync(IEnumerable<T> en) { _set.AddRange(en); return Task.CompletedTask; }

        /// <summary>
        /// Обновляет указанную сущность в контексте данных.
        /// </summary>
        /// <param name="e">Сущность, которую необходимо обновить</param>
        /// <returns>Задача, представляющая завершение операции</returns>
        public Task UpdateAsync(T e) { _set.Update(e); return Task.CompletedTask; }

        /// <summary>
        /// Обновляет набор сущностей в контексте данных.
        /// </summary>
        /// <param name="en">Коллекция сущностей для обновления</param>
        /// <returns>Задача, представляющая завершение операции</returns>
        public Task UpdateRangeAsync(IEnumerable<T> en) { _set.UpdateRange(en); return Task.CompletedTask; }

        /// <summary>
        /// Удаляет сущность с указанным идентификатором, если она существует.
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Задача, представляющая завершение операции</returns>
        public async Task DeleteAsync(Guid id)
        {
            var e = await _set.FindAsync(id);
            if (e != null) _set.Remove(e);
        }

        /// <summary>
        /// Возвращает сущность <see cref="RoomUser"/> по сочетанию идентификаторов пользователя и комнаты.
        /// Метод актуален только для репозитория, работающего с RoomUser (например, специализированной реализации).
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="roomId">Идентификатор комнаты</param>
        /// <returns>Найденная сущность или null</returns>
        public async Task<RoomUser?> GetByUserAndRoomAsync(Guid userId, Guid roomId)
        {
            return await _context.RoomUsers
                .FirstOrDefaultAsync(ru => ru.UserId == userId && ru.RoomId == roomId);
        }

        /// <summary>
        /// Возвращает первую сущность, удовлетворяющую условию <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Условие фильтрации</param>
        /// <returns>Первая подходящая сущность или null</returns>
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Проверяет, существуют ли сущности, удовлетворяющие заданному условию.
        /// </summary>
        /// <param name="predicate">Условие фильтрации</param>
        /// <returns>True, если хотя бы одна сущность удовлетворяет условию; иначе — false</returns>
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _set.AnyAsync(predicate);
        }

        /// <summary>
        /// Удаляет указанную сущность из контекста данных.
        /// </summary>
        /// <param name="entity">Сущность для удаления</param>
        /// <returns>Задача, представляющая завершение операции</returns>
        public Task DeleteAsync(T entity)
        {
            _set.Remove(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Удаляет несколько сущностей из контекста данных.
        /// </summary>
        /// <param name="entities">Коллекция сущностей для удаления</param>
        /// <returns>Задача, представляющая завершение операции</returns>
        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _set.RemoveRange(entities);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Проверяет, существует ли хотя бы одна сущность, удовлетворяющая заданному условию.
        /// Аналогично <see cref="AnyAsync"/>, но используется как вспомогательный метод проверки существования.
        /// </summary>
        /// <param name="predicate">Условие фильтрации</param>
        /// <returns>True, если сущность найдена; иначе — false</returns>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _set.AnyAsync(predicate);
        }
    }
}
