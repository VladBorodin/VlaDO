using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class CompanyHash
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Уникальный идентификатор
        [Required]
        [MaxLength(128)]
        public string Hash { get; set; } = string.Empty; // Хеш компании
    }
}
