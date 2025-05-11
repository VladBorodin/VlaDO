using System.ComponentModel.DataAnnotations;
using VlaDO.Models;

namespace VlaDO.Controllers
{
    public record GenerateTokenDto(AccessLevel AccessLevel, [Range(1, 30)] int DaysValid);
}
