using System.ComponentModel.DataAnnotations;

namespace VlaDO.Models
{
    public class Room
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Владелец комнаты (создатель).  
        /// Если нужно разрешить совместное владение, добавим отдельную таблицу «UserRoom».
        /// </summary>
        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        [Required] 
        public int AccessLevel { get; set; }
        /// <summary>
        /// Название комнаты (необязательно, но удобно в UI)
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }
        /// <summary>
        /// Участники c уровнями доступа
        /// </summary>
        public ICollection<RoomUser> Users { get; set; } = new List<RoomUser>();
        /// <summary>
        /// Коллекция документов, принадлежащих комнате
        /// </summary>
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }

}
