namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для копирования документа в другую комнату.
    /// </summary>
    public class CopyDocumentDto
    {
        /// <summary>
        /// Идентификатор целевой комнаты, в которую нужно скопировать документ.
        /// Если не указан, копия создаётся в той же комнате.
        /// </summary>
        public Guid? TargetRoomId { get; set; }
    }

}
