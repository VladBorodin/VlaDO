using System.ComponentModel.DataAnnotations;
using VlaDO.Models;

namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для обновления уровня доступа пользователя.
    /// </summary>
    public record UpdateAccessDto
    {
        /// <summary>
        /// Идентификатор пользователя, чей доступ обновляется.
        /// </summary>
        [Required]
        public Guid UserId { get; init; }

        /// <summary>
        /// Новый уровень доступа.
        /// </summary>
        [Required]
        public AccessLevel AccessLevel { get; init; }
        public UpdateAccessDto() { }
        public UpdateAccessDto(Guid userId, AccessLevel accessLevel) =>
        (UserId, AccessLevel) = (userId, accessLevel);
    }
}
