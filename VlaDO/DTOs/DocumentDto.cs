namespace VlaDO.DTOs
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserBriefDto CreatedBy { get; set; }
        public RoomBriefDto? Room { get; set; }
        public Guid? PreviousVersionId { get; set; }
        public string? AccessLevel { get; set; }
    }
}
