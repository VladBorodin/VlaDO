namespace VlaDO.Models
{
    public class UserContact
    {
        public Guid UserId { get; set; }
        public Guid ContactId { get; set; }

        public User User { get; set; } = null!;
        public User Contact { get; set; } = null!;
    }

}
