namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для фильтрации комнат при поиске или запросе.
    /// </summary>
    public class RoomFilterDto
    {
        /// <summary>
        /// Название комнаты для фильтрации (поиск по подстроке).
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Уникальный идентификатор комнаты (точный фильтр).
        /// </summary>
        public Guid? RoomId { get; set; }

        /// <summary>
        /// Фильтрация по дате — только комнаты, созданные или изменённые после этой даты.
        /// </summary>
        public DateTime? Since { get; set; }
    }
}
