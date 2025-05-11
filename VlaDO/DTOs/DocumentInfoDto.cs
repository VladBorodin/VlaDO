namespace VlaDO.DTOs
{
    public record DocumentInfoDto( Guid Id,string Name,int Version,Guid? ParentDocId,
        string Hash,string? PrevHash, DateTime CreatedOn, string? Note);

    public record DownloadManyDto(IReadOnlyCollection<Guid> DocumentIds);
}
