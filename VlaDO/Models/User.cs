using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        [Required]
        public Guid ClientTypeId { get; set; }
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
