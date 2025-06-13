using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class PasswordResetToken
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

        public string Token { get; set; } = default!;

        public DateTime ExpiresAt { get; set; }
    }
}
