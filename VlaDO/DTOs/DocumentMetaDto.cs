namespace VlaDO.DTOs
{
    public record DocumentMetaDto(
    Guid Id,
    string Name,
    int Version,
    long Size,        // длина byte[]
    string Extension);
}
