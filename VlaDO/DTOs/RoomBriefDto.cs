namespace VlaDO.DTOs;

public record RoomBriefDto(
    Guid Id,
    string? Title,
    DateTime? LastChange
);
