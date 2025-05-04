using VlaDO.DTOs;

namespace VlaDO.Models
{
    public class RoomUser
    {
        public Guid RoomId { get; set; }
        public Room Room { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public AccessLevel AccessLevel { get; set; } = AccessLevel.Read;
    }
}
