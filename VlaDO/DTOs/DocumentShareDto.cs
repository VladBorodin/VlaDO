using VlaDO.Models;

namespace VlaDO.DTOs
{
    public record DocumentShareDto(
    Guid TokenId,
    Guid UserId,
    string UserName,
    AccessLevel AccessLevel);
}
