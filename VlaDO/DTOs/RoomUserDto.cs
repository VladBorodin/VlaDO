using VlaDO.Models;

namespace VlaDO.DTOs
{
    /// <summary>
    /// Представляет участника комнаты и его уровень доступа.
    /// </summary>
    /// <param name="UserId">Идентификатор пользователя.</param>
    /// <param name="Name">Имя пользователя.</param>
    /// <param name="AccessLevel">Уровень доступа в комнате.</param>
    public record RoomUserDto(Guid UserId, string Name, AccessLevel AccessLevel);
}
