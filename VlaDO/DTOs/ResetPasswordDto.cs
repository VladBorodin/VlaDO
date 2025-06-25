namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для сброса пароля пользователя.
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Токен для подтверждения сброса пароля.
        /// </summary>
        public string Token { get; set; } = default!;
        /// <summary>
        /// Новый пароль.
        /// </summary>
        public string NewPassword { get; set; } = default!;
        /// <summary>
        /// Подтверждение нового пароля.
        /// </summary>
        public string ConfirmPassword { get; set; } = default!;
    }
}
