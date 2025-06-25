using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    /// <summary>
    /// Пользователь системы.
    /// </summary>
    public class User
    {
        /// <summary>Идентификатор пользователя.</summary>
        public Guid Id { get; set; }

        /// <summary>Имя пользователя.</summary>
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        /// <summary>Email пользователя (уникальный).</summary>
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = null!;

        /// <summary>Хэш пароля.</summary>
        [Required]
        public string PasswordHash { get; set; } = null!;

        /// <summary>Дата создания учётной записи.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Комнаты, созданные пользователем.</summary>
        public ICollection<Room> OwnedRooms { get; set; } = new List<Room>();

        /// <summary>Комнаты, к которым пользователь имеет доступ (через RoomUser).</summary>
        public ICollection<RoomUser> Rooms { get; set; } = new List<RoomUser>();

        /// <summary>Контакты пользователя.</summary>
        public ICollection<UserContact> Contacts { get; set; } = new List<UserContact>();
    }

}
