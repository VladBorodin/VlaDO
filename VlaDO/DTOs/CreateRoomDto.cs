using VlaDO.Models;

namespace VlaDO.DTOs.Room;
public class CreateRoomDto
{
    public string Title { get; set; } = string.Empty;
    public AccessLevel DefaultAccessLevel { get; set; }
}
