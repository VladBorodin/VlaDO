using System.ComponentModel.DataAnnotations;
using VlaDO.Models;

namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для обновления уровня доступа пользователя.
    /// </summary>
    public record UpdateAccessDto(
        /// <summary>
        /// Идентификатор пользователя, чей доступ обновляется.
        /// </summary>
        [property: Required] Guid UserId,

        /// <summary>
        /// Новый уровень доступа.
        /// </summary>
        [property: Required] AccessLevel AccessLevel
    );
}
