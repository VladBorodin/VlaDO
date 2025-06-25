using System.ComponentModel.DataAnnotations;

namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для запроса входа в систему.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Адрес электронной почты пользователя.
        /// </summary>
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
