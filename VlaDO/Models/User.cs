using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class User
    {
        public Guid Id { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;
        [Required, MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Room> OwnedRooms { get; set; } = new List<Room>();
        public virtual ICollection<Room> GuestRooms { get; set; } = new List<Room>();
    }
}
