using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    /// <summary>
    /// Представляет комнату (рабочее пространство), в которой хранятся документы и участники с различными уровнями доступа.
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Уникальный идентификатор комнаты.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор владельца комнаты (создателя).
        /// </summary>
        public Guid OwnerId { get; set; }

        /// <summary>
        /// Пользователь, являющийся владельцем комнаты.
        /// </summary>
        public User Owner { get; set; } = null!;

        /// <summary>
        /// Уровень доступа по умолчанию для новых участников.
        /// </summary>
        [Required]
        public int AccessLevel { get; set; }

        /// <summary>
        /// Название комнаты (опционально, используется в UI).
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }

        /// <summary>
        /// Участники комнаты с привязанными уровнями доступа.
        /// </summary>
        public ICollection<RoomUser> Users { get; set; } = new List<RoomUser>();

        /// <summary>
        /// Документы, связанные с этой комнатой.
        /// </summary>
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
