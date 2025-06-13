namespace VlaDO.DTOs
{
    public class CreateDocumentDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Note { get; set; }
        public Guid? RoomId { get; set; }
        public IFormFile? File { get; set; }
    }
}
