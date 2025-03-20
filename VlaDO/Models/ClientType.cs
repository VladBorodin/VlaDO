using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class ClientType
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Уникальный идентификатор типа

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Название типа (например, "Физическое лицо", "Компания")
    }
}
