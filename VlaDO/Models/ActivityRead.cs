namespace VlaDO.Models
{
    public class ActivityRead
    {
        public Guid ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
