/// <summary>
/// DTO для загрузки нескольких документов по их идентификаторам.
/// </summary>
public class DownloadManyDto
{
    /// <summary>
    /// Список идентификаторов документов, которые нужно скачать.
    /// </summary>
    public List<Guid> DocumentIds { get; set; } = new();
}
