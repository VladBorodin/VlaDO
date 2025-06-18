using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class DocumentToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid DocumentId { get; set; }
        public Document Document { get; set; } = null!;
        [MaxLength(64)]
        public string Token { get; set; } = Guid.NewGuid().ToString("N");
        public AccessLevel AccessLevel { get; set; } = AccessLevel.Read;
        public DateTime? ExpiresAt { get; set; }
    }
}
