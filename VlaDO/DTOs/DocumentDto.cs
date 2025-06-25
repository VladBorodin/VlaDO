namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO-модель для отображения информации о документе.
    /// </summary>
    public class DocumentDto
    {
        /// <summary>
        /// Уникальный идентификатор документа.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Название документа.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Версия документа (инкрементируется при обновлении).
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Ветка форка, к которой относится этот документ.
        /// </summary>
        public string ForkPath { get; set; } = "0";

        /// <summary>
        /// Дата и время создания документа.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Краткая информация о пользователе, создавшем документ.
        /// </summary>
        public UserBriefDto CreatedBy { get; set; }

        /// <summary>
        /// Информация о комнате, в которой находится документ (если есть).
        /// </summary>
        public RoomBriefDto? Room { get; set; }

        /// <summary>
        /// Идентификатор предыдущей версии документа (если есть).
        /// </summary>
        public Guid? PreviousVersionId { get; set; }

        /// <summary>
        /// Уровень доступа текущего пользователя к документу, если он существует (например: "Read", "Edit").
        /// </summary>
        public string? AccessLevel { get; set; }
    }
}