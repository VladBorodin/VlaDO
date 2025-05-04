using VlaDO.Models;

namespace VlaDO.Controllers.Rooms
{
    public record RoomUserDto(Guid UserId, string Name, AccessLevel AccessLevel);
}
