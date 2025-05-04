using VlaDO.Models;

namespace VlaDO.Controllers.Rooms
{
    public record AddUserToRoomDto(Guid UserId, AccessLevel AccessLevel);
}
