using System.Text.Json;
using VlaDO.Models;

namespace VlaDO.DTOs
{
    /// <summary>Представление активности в API-ответе</summary>
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
    public record ActivityDtoExt(ActivityDto Activity, bool IsRead);
    public record PagedResult<T>(IReadOnlyList<T> Items, int TotalPages);

}
