namespace VlaDO.DTOs
{
    /// <summary>
    /// Краткая информация о комнате.
    /// </summary>
    /// <param name="Id">Уникальный идентификатор комнаты.</param>
    /// <param name="Title">Название комнаты.</param>
    /// <param name="LastChange">Дата последнего изменения (опционально).</param>
    public record RoomBriefDto(
        Guid Id,
        string? Title,
        DateTime? LastChange
    );
}
