using System.ComponentModel.DataAnnotations;
using VlaDO.Models;

namespace VlaDO.Controllers
{
    /// <summary>
    /// DTO для генерации токена доступа к документу.
    /// </summary>
    /// <param name="AccessLevel">Уровень доступа, предоставляемый токеном.</param>
    /// <param name="DaysValid">Срок действия токена в днях (от 1 до 30).</param>
    public record GenerateTokenDto(AccessLevel AccessLevel, [Range(1, 30)] int DaysValid);
}
