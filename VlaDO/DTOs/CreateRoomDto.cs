using VlaDO.Models;

namespace VlaDO.DTOs.Room;
public class CreateRoomDto
{
    public string Title { get; set; } = string.Empty;
    /// <summary>Уровень доступа по умолчанию для приглашённых (0-Read, 1-Edit, 2-Full)</summary>
    public AccessLevel DefaultAccessLevel { get; set; }
}
