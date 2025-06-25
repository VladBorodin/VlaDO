using VlaDO.DTOs;

namespace VlaDO.Services
{
    /// <summary>
    /// Сервис для генерации, валидации и управления токенами сброса пароля.
    /// </summary>
    public interface IPasswordResetService
    {
        /// <summary>
        /// Генерирует токен сброса пароля для указанного пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя, которому требуется сброс пароля.</param>
        /// <returns>Строка с токеном для сброса пароля.</returns>
        Task<string> GeneratePasswordResetTokenAsync(Guid userId);

        /// <summary>
        /// Проверяет корректность токена сброса пароля.
        /// </summary>
        /// <param name="token">Токен, подлежащий валидации.</param>
        /// <param name="userId">Идентификатор пользователя, для которого предназначен токен.</param>
        /// <returns>True, если токен валиден; иначе — false.</returns>
        Task<bool> ValidatePasswordResetTokenAsync(string token, Guid userId);

        /// <summary>
        /// Создаёт токен сброса пароля по данным запроса (email).
        /// </summary>
        /// <param name="dto">DTO с email пользователя, запросившего сброс пароля.</param>
        /// <returns>Сгенерированный токен сброса пароля в виде строки.</returns>
        Task<string> CreateResetTokenAsync(ForgotPasswordRequestDto dto);
    }
}
