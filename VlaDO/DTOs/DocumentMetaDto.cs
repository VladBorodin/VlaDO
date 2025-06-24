namespace VlaDO.DTOs
{
    public record DocumentMetaDto(
        Guid Id,
        string Name,
        int Version,
        long Size,
        string Extension,
        string? RoomTitle,
        Guid? RoomId,
        string CreatedBy,
        Guid CreatedById,
        DateTime? CreatedAt,
        string? Note,
        string ForkPath);
}
