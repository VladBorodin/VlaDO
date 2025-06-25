using VlaDO.Models;
using VlaDO.Services;

/// <summary>
/// Служит для накопления и логирования групповой активности в одном сообщении.
/// Используется в блоке using для автоматической отправки при завершении.
/// </summary>
public sealed class BulkActivityScope : IAsyncDisposable
{
    /// <summary>
    /// Список мета-объектов, которые будут включены в лог активности.
    /// </summary>
    private readonly List<object> _meta = new();

    /// <summary>
    /// Логгер активности, используемый для записи.
    /// </summary>
    private readonly IActivityLogger _logger;

    /// <summary>
    /// Тип активности, который будет зафиксирован.
    /// </summary>
    private readonly ActivityType _type;

    /// <summary>
    /// Идентификатор пользователя, выполняющего действие.
    /// </summary>
    private readonly Guid _author;

    /// <summary>
    /// Идентификатор объекта, к которому относится активность (опционально).
    /// </summary>
    private readonly Guid? _subjectId;

    /// <summary>
    /// Создаёт новый экземпляр BulkActivityScope для отложенного логирования.
    /// </summary>
    /// <param name="logger">Логгер активности.</param>
    /// <param name="type">Тип логируемой активности.</param>
    /// <param name="author">ID автора действия.</param>
    /// <param name="subjectId">ID объекта действия (опционально).</param>
    public BulkActivityScope(
        IActivityLogger logger,
        ActivityType type,
        Guid author,
        Guid? subjectId = null)
    {
        _logger = logger;
        _type = type;
        _author = author;
        _subjectId = subjectId;
    }

    /// <summary>
    /// Добавляет метаинформацию об одном элементе действия (например, документе).
    /// </summary>
    /// <param name="metaItem">Анонимный объект с нужными полями.</param>
    public void Add(object metaItem) => _meta.Add(metaItem);

    /// <summary>
    /// Асинхронно записывает лог, если накоплены элементы.
    /// Вызывается автоматически при завершении using-блока.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_meta.Count == 0) return;

        await _logger.LogAsync(
            type: _type,
            authorId: _author,
            subjectId: _subjectId,
            meta: new { Items = _meta, Count = _meta.Count }
        );
    }
}
