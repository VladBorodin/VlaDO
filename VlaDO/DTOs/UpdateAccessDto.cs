using System.ComponentModel.DataAnnotations;
using VlaDO.Models;

namespace VlaDO.DTOs
{
    public record UpdateAccessDto(
    Guid UserId,
    AccessLevel AccessLevel);
}
