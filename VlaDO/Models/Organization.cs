using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; } // Название группы/компании
        public Guid ClientTypeId { get; set; } // Тип (Компания, Группа, Проект)
        public string? RegistrationCode { get; set; } // Лицензионный код (если компания)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
    }
}
