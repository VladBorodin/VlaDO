using VlaDO.Models;
using VlaDO.Services;

public sealed class BulkActivityScope : IAsyncDisposable
{
    private readonly List<object> _meta = new();
    private readonly IActivityLogger _logger;
    private readonly ActivityType _type;
    private readonly Guid _author;
    private readonly Guid? _subjectId;

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
    /// Вызываем из цикла для каждого удаляемого/архивируемого итема.
    /// Можно передавать анонимные объекты с нужными полями.
    /// </summary>
    public void Add(object metaItem) => _meta.Add(metaItem);

    /// <summary>
    /// При выходе из using-блока — если что-то накоплено — отправляем одно сообщение.
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
