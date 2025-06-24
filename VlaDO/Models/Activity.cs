namespace VlaDO.Models
{
    public class Activity
    {
        public Guid Id { get; set; }
        public ActivityType Type { get; set; }
        public Guid? UserId { get; set; }
        public Guid? AuthorId { get; set; }
        public Guid? RoomId { get; set; }
        public Guid? DocumentId { get; set; }
        public string? PayloadJson { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
