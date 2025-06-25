using System.ComponentModel.DataAnnotations;
using VlaDO.Models;

namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для добавления пользователя в комнату с определённым уровнем доступа.
    /// </summary>
    public class AddUserToRoomDto
    {
        /// <summary>
        /// Идентификатор пользователя, которого нужно добавить в комнату.
        /// </summary>
        [Required] public Guid UserId { get; set; }
        /// <summary>
        /// Уровень доступа, который будет назначен пользователю.
        /// </summary>
        [Required] public AccessLevel AccessLevel { get; set; }
    }
}
