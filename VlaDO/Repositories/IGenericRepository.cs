using System.Linq.Expressions;

namespace VlaDO.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Получает все сущности данного типа.
        /// </summary>
        /// <returns>Коллекция всех сущностей</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Получает сущность по идентификатору с возможностью включения связанных сущностей.
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="includes">Выражения для включения связанных сущностей</param>
        /// <returns>Сущность или null, если не найдена</returns>
        Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
        
        /// <summary>
        /// Получает страницу сущностей, отсортированных по порядку в базе данных.
        /// </summary>
        /// <param name="pageNumber">Номер страницы (начиная с 1)</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <returns>Коллекция сущностей на заданной странице</returns>
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Ищет сущности, соответствующие заданному условию, с возможностью сортировки и включения.
        /// </summary>
        /// <param name="predicate">Условие фильтрации</param>
        /// <param name="orderBy">Функция сортировки (опционально)</param>
        /// <param name="include">Выражения для включения связанных сущностей</param>
        /// <returns>Коллекция найденных сущностей</returns>
        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] include);

        /// <summary>
        /// Проверяет существование сущности по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>True, если сущность существует; иначе — false</returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Добавляет новую сущность.
        /// </summary>
        /// <param name="entity">Сущность для добавления</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Добавляет коллекцию сущностей.
        /// </summary>
        /// <param name="entities">Сущности для добавления</param>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Обновляет указанную сущность.
        /// </summary>
        /// <param name="entity">Сущность для обновления</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Обновляет коллекцию сущностей.
        /// </summary>
        /// <param name="entities">Сущности для обновления</param>
        Task UpdateRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Удаляет сущность по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Возвращает первую сущность, удовлетворяющую условию, или null.
        /// </summary>
        /// <param name="predicate">Условие фильтрации</param>
        /// <returns>Первая сущность или null</returns>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Проверяет, существует ли хотя бы одна сущность, соответствующая условию.
        /// </summary>
        /// <param name="predicate">Условие фильтрации</param>
        /// <returns>True, если сущность существует; иначе — false</returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Удаляет сущность из контекста данных.
        /// </summary>
        /// <param name="entity">Сущность для удаления</param>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Удаляет несколько сущностей из контекста данных.
        /// </summary>
        /// <param name="entities">Сущности для удаления</param>
        Task DeleteRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Проверяет, существует ли сущность, удовлетворяющая условию.
        /// </summary>
        /// <param name="predicate">Условие фильтрации</param>
        /// <returns>True, если сущность найдена; иначе — false</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}
