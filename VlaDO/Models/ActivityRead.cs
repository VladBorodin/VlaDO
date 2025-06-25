namespace VlaDO.Models
{
    /// <summary>
    /// Отображает факт прочтения пользователем конкретной активности.
    /// Используется для отслеживания непрочитанных уведомлений или событий.
    /// </summary>
    public class ActivityRead
    {
        /// <summary>
        /// Идентификатор активности, которая была прочитана.
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Навигационное свойство для доступа к связанной активности.
        /// </summary>
        public Activity Activity { get; set; } = null!;

        /// <summary>
        /// Идентификатор пользователя, который прочитал активность.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Навигационное свойство для доступа к пользователю.
        /// </summary>
        public User User { get; set; } = null!;
    }
}
