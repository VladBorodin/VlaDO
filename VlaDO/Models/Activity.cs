namespace VlaDO.Models
{
    /// <summary>
    /// Запись активности, отражающая действия пользователей в системе (например, приглашения, изменения, загрузки).
    /// </summary>
    public class Activity
    {
        /// <summary>
        /// Уникальный идентификатор активности.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тип активности (например, приглашение, загрузка, отклонение).
        /// </summary>
        public ActivityType Type { get; set; }

        /// <summary>
        /// Пользователь, к которому относится активность (например, приглашённый пользователь).
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Автор действия (тот, кто выполнил активность, например, пригласил).
        /// </summary>
        public Guid? AuthorId { get; set; }

        /// <summary>
        /// Идентификатор комнаты, с которой связана активность (если применимо).
        /// </summary>
        public Guid? RoomId { get; set; }

        /// <summary>
        /// Идентификатор документа, с которым связана активность (если применимо).
        /// </summary>
        public Guid? DocumentId { get; set; }

        /// <summary>
        /// Дополнительные метаданные в формате JSON. Используется для хранения кастомной информации.
        /// </summary>
        public string? PayloadJson { get; set; }

        /// <summary>
        /// Дата и время создания активности.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
