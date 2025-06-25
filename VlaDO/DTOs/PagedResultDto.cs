namespace VlaDO.DTOs
{
    /// <summary>
    /// Обёртка для постраничного результата.
    /// </summary>
    /// <typeparam name="T">Тип элементов в списке.</typeparam>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// Коллекция элементов текущей страницы.
        /// </summary>
        public IEnumerable<T> Items { get; init; } = Array.Empty<T>();

        /// <summary>
        /// Общее количество элементов.
        /// </summary>
        public int Total { get; init; }

        /// <summary>
        /// Номер текущей страницы (начиная с 1).
        /// </summary>
        public int Page { get; init; }

        /// <summary>
        /// Количество элементов на странице.
        /// </summary>
        public int PageSize { get; init; }
    }
}
