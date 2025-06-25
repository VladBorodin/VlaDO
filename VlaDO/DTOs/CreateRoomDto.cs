using VlaDO.Models;

namespace VlaDO.DTOs.Room
{
    /// <summary>
    /// DTO-модель для создания новой комнаты.
    /// </summary>
    public class CreateRoomDto
    {
        /// <summary>
        /// Название комнаты, которое будет отображаться пользователям.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Уровень доступа, который по умолчанию получают новые участники комнаты.
        /// </summary>
        public AccessLevel DefaultAccessLevel { get; set; }
    }
}
