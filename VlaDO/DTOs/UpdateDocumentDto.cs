namespace VlaDO.DTOs
{
    public class UpdateDocumentDto
    {
        public string? Name { get; set; }
        public string? Note { get; set; }
        public Guid? RoomId { get; set; } // вдруг пользователь перенёс документ
        public Guid? ParentDocId { get; set; }
        public IFormFile File { get; set; } = null!;
    }

}
