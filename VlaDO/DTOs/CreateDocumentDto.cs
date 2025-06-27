namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO-модель для создания нового документа.
    /// </summary>
    public class CreateDocumentDto
    {
        /// <summary>
        /// Название документа. Используется в отображении и идентификации.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Необязательная заметка или описание, связанное с документом.
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Идентификатор комнаты, в которую добавляется документ.
        /// Если не указано — документ может быть добавлен в общий или временный контекст.
        /// </summary>
        public Guid? RoomId { get; set; }

        /// <summary>
        /// Загружаемый файл, представляющий содержимое документа.
        /// </summary>
        public IFormFile? File { get; set; }

        /// <summary>
        /// Хеш предыдущей версии документа (если создается новая версия на основе существующей).
        /// Используется для проверки целостности цепочки версий.
        /// </summary>
        public string? PrevHash { get; set; }

        /// <summary>
        /// Если создаём НОВУЮ ВЕРСИЮ — сюда передаётся Id исходного (родительского) документа.
        /// null  → это «первая» версия (новый корневой документ).
        /// </summary>
        public Guid? ParentDocId { get; set; }
    }
}
