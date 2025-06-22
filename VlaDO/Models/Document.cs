using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class Document
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        [Required]
        public Guid CreatedBy { get; set; }
        [Required]
        public int Version { get; set; } = 1; 
        [Required, MaxLength(128)]
        public string ForkPath { get; set; } = "0";
        public Guid? ParentDocId { get; set; }
        public byte[]? Data { get; set; }
        [MaxLength(1024)]
        public string? Note { get; set; }
        [Required, MaxLength(128)]
        public string Hash { get; set; } = string.Empty;
        [MaxLength(128)]
        public string? PrevHash { get; set; }
        public Guid? RoomId { get; set; }
        public Room? Room { get; set; }
        /// <summary>
        /// Публичные токены
        /// </summary>
        public ICollection<DocumentToken> Tokens { get; set; } = new List<DocumentToken>();
    }
}