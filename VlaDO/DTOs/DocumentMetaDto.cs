namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO с расширенными метаданными документа, используется для отображения или экспорта сведений о документе.
    /// </summary>
    /// <param name="Id">Уникальный идентификатор документа.</param>
    /// <param name="Name">Название документа.</param>
    /// <param name="Version">Номер версии документа.</param>
    /// <param name="Size">Размер документа в байтах.</param>
    /// <param name="Extension">Расширение файла (например, ".pdf", ".docx").</param>
    /// <param name="RoomTitle">Название комнаты, к которой принадлежит документ (если указано).</param>
    /// <param name="RoomId">Идентификатор комнаты (если документ привязан к ней).</param>
    /// <param name="CreatedBy">Имя пользователя, создавшего документ.</param>
    /// <param name="CreatedById">Идентификатор пользователя, создавшего документ.</param>
    /// <param name="CreatedAt">Дата и время создания документа.</param>
    /// <param name="Note">Примечание или комментарий к документу.</param>
    /// <param name="ForkPath">Путь ветвления, отражающий родословную документа (например, "0-3-1").</param>
    public record DocumentMetaDto(
        Guid Id,
        string Name,
        int Version,
        long Size,
        string Extension,
        string? RoomTitle,
        Guid? RoomId,
        string CreatedBy,
        Guid CreatedById,
        DateTime? CreatedAt,
        string? Note,
        string ForkPath
    );
}
