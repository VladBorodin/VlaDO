using System.Text.Json;
using VlaDO.Models;

namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для представления активности пользователя.
    /// </summary>
    /// <param name="Id">Идентификатор активности.</param>
    /// <param name="Meta">Мета-данные в формате JSON (десериализованные).</param>
    /// <param name="Details">Строковое представление мета-данных (оригинальный JSON).</param>
    /// <param name="CreatedAt">Дата и время создания активности.</param>
    /// <param name="Type">Тип активности.</param>
    /// <param name="RoomId">Необязательный идентификатор комнаты, связанной с активностью.</param>
    public record ActivityDto
    (
        Guid Id,
        JsonDocument? Meta,
        string? Details,
        DateTime CreatedAt,
        ActivityType Type,
        Guid? RoomId = null
    )

    {
        /// <summary>
        /// Создаёт DTO активности из сущности Activity.
        /// </summary>
        /// <param name="a">Сущность активности.</param>
        public ActivityDto(Activity a) : this(
            a.Id,
            string.IsNullOrWhiteSpace(a.PayloadJson) ? null : JsonDocument.Parse(a.PayloadJson),
            a.PayloadJson,
            a.CreatedAt,
            a.Type,
            a.RoomId
        )
        { }
    }

    /// <summary>
    /// DTO активности с признаком прочитанности.
    /// </summary>
    /// <param name="Activity">Объект активности.</param>
    /// <param name="IsRead">Признак того, была ли активность прочитана.</param>
    public record ActivityDtoExt(ActivityDto Activity, bool IsRead);

    /// <summary>
    /// Универсальный контейнер для постраничных результатов.
    /// </summary>
    /// <typeparam name="T">Тип элементов.</typeparam>
    /// <param name="Items">Список элементов текущей страницы.</param>
    /// <param name="TotalPages">Общее количество страниц.</param>
    public record PagedResult<T>(IReadOnlyList<T> Items, int TotalPages);

}
