namespace VlaDO.DTOs
{
    public record RoomWithAccessDto(
        Guid Id,
        string? Title,
        DateTime? LastChange,
        string AccessLevel
    ) : RoomBriefDto(Id, Title, LastChange);
}
