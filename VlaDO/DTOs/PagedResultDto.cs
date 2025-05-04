namespace VlaDO.DTOs
{
    public class PagedResultDto<T>
    {
        public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
        public int Total { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}
