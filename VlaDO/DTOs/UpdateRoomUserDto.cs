using System.ComponentModel.DataAnnotations;
using VlaDO.Models;

namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для обновления уровня доступа пользователя в комнате.
    /// </summary>
    public class UpdateRoomUserDto
    {
        /// <summary>
        /// Новый уровень доступа.
        /// </summary>
        [Required]
        public AccessLevel AccessLevel { get; set; }
    }
}
