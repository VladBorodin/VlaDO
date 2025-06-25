using System.ComponentModel.DataAnnotations;

namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для регистрации нового пользователя.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Электронная почта пользователя.
        /// </summary>
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Подтверждение пароля.
        /// </summary>
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
