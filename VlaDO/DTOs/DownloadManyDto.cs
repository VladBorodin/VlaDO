namespace VlaDO.DTOs.Document;
public record DownloadManyDto(IReadOnlyCollection<Guid> DocumentIds);