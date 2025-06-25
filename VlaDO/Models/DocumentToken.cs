using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    /// <summary>
    /// Представляет токен доступа пользователя к документу.
    /// </summary>
    public class DocumentToken
    {
        /// <summary>
        /// Уникальный идентификатор токена.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Идентификатор пользователя, получившего токен.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Навигационное свойство — пользователь, которому выдан токен.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Идентификатор документа, к которому выдан токен.
        /// </summary>
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Навигационное свойство — документ, к которому выдан токен.
        /// </summary>
        public Document Document { get; set; } = null!;

        /// <summary>
        /// Уникальный строковый токен доступа (до 64 символов).
        /// </summary>
        [MaxLength(64)]
        public string Token { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Уровень доступа, предоставляемый этим токеном.
        /// </summary>
        public AccessLevel AccessLevel { get; set; } = AccessLevel.Read;

        /// <summary>
        /// Время истечения срока действия токена (если указано).
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}