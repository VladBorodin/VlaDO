using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    /// <summary>
    /// Представляет документ, хранящийся в системе. Может принадлежать комнате и иметь историю версий.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Уникальный идентификатор документа.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Название документа (до 255 символов).
        /// </summary>
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Дата и время создания документа (UTC).
        /// </summary>
        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Идентификатор пользователя, создавшего документ.
        /// </summary>
        [Required]
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Версия документа (начинается с 1).
        /// </summary>
        [Required]
        public int Version { get; set; } = 1;

        /// <summary>
        /// Путь форка (цепочка версий, например: "0", "0.1", "0.1.2").
        /// </summary>
        [Required, MaxLength(128)]
        public string ForkPath { get; set; } = "0";

        /// <summary>
        /// Идентификатор родительского документа, если это форк.
        /// </summary>
        public Guid? ParentDocId { get; set; }

        /// <summary>
        /// Двоичные данные файла.
        /// </summary>
        public byte[]? Data { get; set; }

        /// <summary>
        /// Примечание или описание к документу (до 1024 символов).
        /// </summary>
        [MaxLength(1024)]
        public string? Note { get; set; }

        /// <summary>
        /// Хеш-сумма текущей версии документа.
        /// </summary>
        [Required, MaxLength(128)]
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// Хеш предыдущей версии документа.
        /// </summary>
        [MaxLength(128)]
        public string? PrevHash { get; set; }

        /// <summary>
        /// Идентификатор комнаты, к которой принадлежит документ (если есть).
        /// </summary>
        public Guid? RoomId { get; set; }

        /// <summary>
        /// Навигационное свойство для комнаты.
        /// </summary>
        public Room? Room { get; set; }

        /// <summary>
        /// Список токенов доступа к документу.
        /// </summary>
        public ICollection<DocumentToken> Tokens { get; set; } = new List<DocumentToken>();
    }
}
