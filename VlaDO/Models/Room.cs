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
        public virtual User Owner { get; set; } = null!;

        /// <summary>
        /// Название комнаты (необязательно, но удобно в UI)
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }

        /// <summary>
        /// Коллекция документов, принадлежащих комнате
        /// </summary>
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<User> Guests { get; set; } = new List<User>();
    }

}
