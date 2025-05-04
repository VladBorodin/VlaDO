using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class User
    {
        public Guid Id { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = null!;
        [Required]
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Room> OwnedRooms { get; set; } = new List<Room>();
        public ICollection<RoomUser> Rooms { get; set; } = new List<RoomUser>();
    }
}
