using System.ComponentModel.DataAnnotations;
using VlaDO.DTOs;

namespace VlaDO.Models
{
    public class DocumentToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DocumentId { get; set; }
        public Document Document { get; set; } = null!;
        [MaxLength(64)]
        public string Token { get; set; } = null!;
        public AccessLevel AccessLevel { get; set; } = AccessLevel.Read;
        public DateTime ExpiresAt { get; set; }
    }
}
