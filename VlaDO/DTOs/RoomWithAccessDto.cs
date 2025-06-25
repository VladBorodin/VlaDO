namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для отображения комнаты с учётом уровня доступа пользователя.
    /// Наследуется от <see cref="RoomBriefDto"/>.
    /// </summary>
    /// <param name="Id">Идентификатор комнаты.</param>
    /// <param name="Title">Название комнаты.</param>
    /// <param name="LastChange">Дата последнего изменения.</param>
    /// <param name="AccessLevel">Уровень доступа текущего пользователя (Read, Write, Full и т.д.).</param>
    public record RoomWithAccessDto(
        Guid Id,
        string? Title,
        DateTime? LastChange,
        string AccessLevel
    ) : RoomBriefDto(Id, Title, LastChange);
}
