namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для переименования сущности, например, комнаты или документа.
    /// </summary>
    public class RenameDto
    {
        /// <summary>
        /// Новое имя.
        /// </summary>
        public string Name { get; set; }
    }
}
