using VlaDO.DTOs;
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

    // ───────── «шары» между пользователями ─────────
    public async Task<DocumentShareDto[]> GetSharesAsync(Guid docId)
    {
        var list = await _uow.Tokens
            .FindAsync(t => t.DocumentId == docId && t.UserId != Guid.Empty,
                       null, t => t.Document);

        // подгружаем имена пользователей одним запросом
        var uids = list.Select(t => t.UserId).Distinct().ToList();
        var users = await _uow.Users
            .FindAsync(u => uids.Contains(u.Id));

        return list.Select(t => new DocumentShareDto(
                t.Id,
                t.UserId,
                users.First(u => u.Id == t.UserId).Name,
                t.AccessLevel))
            .ToArray();
    }

    public async Task<DocumentShareDto> UpsertShareAsync(
        Guid docId, Guid userId, AccessLevel level)
    {
        var tok = await _uow.Tokens
            .FirstOrDefaultAsync(t => t.DocumentId == docId &&
                                      t.UserId == userId);

        if (tok is null)
        {
            tok = new DocumentToken
            {
                DocumentId = docId,
                UserId = userId,
                Token = Guid.NewGuid().ToString("N"),
                AccessLevel = level,
                ExpiresAt = DateTime.UtcNow.AddYears(5)
            };
            await _uow.Tokens.AddAsync(tok);
        }
        else
        {
            tok.AccessLevel = level;
        }

        await _uow.CommitAsync();

        var user = await _uow.Users.GetByIdAsync(userId);
        return new DocumentShareDto(tok.Id, userId, user!.Name, level);
    }

    public async Task RevokeShareAsync(Guid tokenId)
    {
        await _uow.Tokens.DeleteAsync(tokenId);
        await _uow.CommitAsync();
    }
}