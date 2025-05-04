using VlaDO.Models;

namespace VlaDO.DTOs
{
    public record RoomUserDto(Guid UserId, string Name, AccessLevel AccessLevel);
}
