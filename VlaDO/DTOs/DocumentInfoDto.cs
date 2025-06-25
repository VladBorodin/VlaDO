namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для базовой информации о документе, используемой в служебных операциях (например, при скачивании).
    /// </summary>
    /// <param name="Id">Уникальный идентификатор документа.</param>
    /// <param name="Name">Название документа.</param>
    /// <param name="Version">Версия документа.</param>
    /// <param name="ParentDocId">Идентификатор родительского документа (если это форк или новая версия).</param>
    /// <param name="Hash">Хеш текущей версии документа.</param>
    /// <param name="PrevHash">Хеш предыдущей версии (если есть).</param>
    /// <param name="CreatedOn">Дата и время создания документа.</param>
    /// <param name="Note">Описание или примечание к документу (опционально).</param>
    /// <param name="ForkPath">Путь ветвления документа (например, "0-2").</param>
    public record DocumentInfoDto(
        Guid Id,
        string Name,
        int Version,
        Guid? ParentDocId,
        string Hash,
        string? PrevHash,
        DateTime CreatedOn,
        string? Note,
        string ForkPath
    );

    /// <summary>
    /// DTO для запроса скачивания нескольких документов по их идентификаторам.
    /// </summary>
    /// <param name="DocumentIds">Список идентификаторов документов для скачивания.</param>
    public record DownloadManyDto(IReadOnlyCollection<Guid> DocumentIds);
}
