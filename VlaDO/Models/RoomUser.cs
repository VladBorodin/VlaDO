namespace VlaDO.Models
{
    /// <summary>
    /// Промежуточная сущность для связи пользователей с комнатами и назначения уровня доступа.
    /// </summary>
    public class RoomUser
    {
        /// <summary>
        /// Идентификатор комнаты.
        /// </summary>
        public Guid RoomId { get; set; }

        /// <summary>
        /// Навигационное свойство комнаты.
        /// </summary>
        public Room Room { get; set; } = null!;

        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Навигационное свойство пользователя.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Уровень доступа пользователя в комнате.
        /// </summary>
        public AccessLevel AccessLevel { get; set; } = AccessLevel.Read;
    }
}
