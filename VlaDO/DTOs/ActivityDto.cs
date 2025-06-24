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
        ActivityType Type
    )
    {
        public ActivityDto(Activity a) : this(
            a.Id,
            string.IsNullOrWhiteSpace(a.PayloadJson) ? null : JsonDocument.Parse(a.PayloadJson),
            a.PayloadJson,
            a.CreatedAt,
            a.Type
        )
        { }
    }
}
