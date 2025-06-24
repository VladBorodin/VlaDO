using Microsoft.EntityFrameworkCore;

namespace VlaDO.Models
{
    public class UserContact
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid ContactId { get; set; }

        public User User { get; set; } = null!;
        public User Contact { get; set; } = null!;
    }

}
