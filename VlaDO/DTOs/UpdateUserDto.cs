namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для обновления имени и email пользователя.
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// Новое имя пользователя.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Новый email пользователя.
        /// </summary>
        public string Email { get; set; }
    }
}
