using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    /// <summary>
    /// Представляет токен сброса пароля, связанный с конкретным пользователем.
    /// </summary>
    public class PasswordResetToken
    {
        /// <summary>
        /// Уникальный идентификатор токена сброса.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор пользователя, которому выдан токен.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Навигационное свойство — пользователь, которому выдан токен.
        /// </summary>
        public User User { get; set; } = default!;

        /// <summary>
        /// Уникальная строка токена сброса пароля.
        /// </summary>
        public string Token { get; set; } = default!;

        /// <summary>
        /// Время истечения срока действия токена.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
