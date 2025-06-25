namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO-запрос для восстановления пароля пользователя.
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        /// <summary>
        /// Email пользователя, на который будет отправлена ссылка для сброса пароля.
        /// </summary>
        public string Email { get; set; } = default!;
    }
}