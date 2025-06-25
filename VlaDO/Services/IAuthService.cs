namespace VlaDO.Services
{
    using VlaDO.DTOs;

    /// <summary>
    /// Сервис аутентификации и регистрации пользователей.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Регистрирует нового пользователя на основе переданных данных.
        /// </summary>
        /// <param name="dto">DTO с данными для регистрации (имя, email, пароль и др.).</param>
        Task RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Аутентифицирует пользователя и возвращает JWT-токен при успешной проверке.
        /// </summary>
        /// <param name="dto">DTO с данными для входа (email и пароль).</param>
        /// <returns>JWT-токен, если вход успешен; иначе — <c>null</c>.</returns>
        Task<string?> LoginAsync(LoginDto dto);
    }
}
