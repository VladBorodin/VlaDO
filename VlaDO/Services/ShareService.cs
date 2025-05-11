using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services;

public class ShareService : IShareService
{
    private readonly IUnitOfWork _uow;
    public ShareService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<string> ShareDocumentAsync(Guid docId, AccessLevel level, TimeSpan ttl)
    {
        var token = Guid.NewGuid().ToString("N");

        var doc = await _uow.Documents.GetByIdAsync(docId)
                   ?? throw new KeyNotFoundException("Документ не найден");

        var dt = new DocumentToken
        {
            DocumentId = docId,
            Token = token,
            AccessLevel = level,
            ExpiresAt = DateTime.UtcNow + ttl
        };

        await _uow.Tokens.AddAsync(dt);
        await _uow.CommitAsync();
        return token;
    }

    public async Task RevokeTokenAsync(string token)
    {
        var dt = (await _uow.Tokens.FindAsync(t => t.Token == token)).FirstOrDefault();
        if (dt != null)
        {
            await _uow.Tokens.DeleteAsync(dt.Id);
            await _uow.CommitAsync();
        }
    }

    public async Task<(byte[] bytes, string name, string ctype, Guid roomId)> DownloadByTokenAsync(string token)
    {
        var dt = (await _uow.Tokens.FindAsync(
                      t => t.Token == token && t.ExpiresAt > DateTime.UtcNow,
                      null,
                      t => t.Document)).FirstOrDefault();

        if (dt == null || dt.Document == null)
            throw new KeyNotFoundException("Токен недействителен или срок истёк");

        var doc = dt.Document;

        return (
            doc.Data ?? throw new InvalidOperationException("Документ повреждён или пуст"),
            doc.Name,
            "application/octet-stream",
            doc.RoomId ?? throw new InvalidOperationException("Документ не привязан к комнате")
        );
    }
}