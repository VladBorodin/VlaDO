using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class Document
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Уникальный идентификатор документа

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty; // Название документа

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow; // Дата создания

        [Required]
        public Guid CreatedBy { get; set; } // Кто создал документ

        [Required]
        public int Version { get; set; } = 1; // Номер версии документа

        public Guid? ParentDoc { get; set; } // Ссылка на предыдущую версию (NULL для первой)

        public byte[]? Data { get; set; } // Файл документа (BLOB) или NULL, если хранится во внешнем хранилище

        [MaxLength(1024)]
        public string? Note { get; set; } // Примечания

        [MaxLength(512)]
        public string? Allowed { get; set; } // Зашифрованные права доступа

        [Required]
        [MaxLength(128)]
        public string Hash { get; set; } = string.Empty; // Хеш текущей версии

        [MaxLength(128)]
        public string? PrevHash { get; set; } // Хеш предыдущей версии (для контроля целостности)
        public Guid RoomId { get; set; }
    }
}