namespace VlaDO.Models
{
    /// <summary>
    /// Связь между пользователями в виде контактов.
    /// </summary>
    public class UserContact
    {
        /// <summary>Идентификатор связи.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>ID владельца списка контактов.</summary>
        public Guid UserId { get; set; }

        /// <summary>ID контакта (другого пользователя).</summary>
        public Guid ContactId { get; set; }

        /// <summary>Ссылка на владельца.</summary>
        public User User { get; set; } = null!;

        /// <summary>Ссылка на контакт.</summary>
        public User Contact { get; set; } = null!;
    }
}
