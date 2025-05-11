using VlaDO.Models;

namespace VlaDO.Services;
public interface IShareService
{
    Task<string> ShareDocumentAsync(Guid docId, AccessLevel level, TimeSpan ttl);
    Task RevokeTokenAsync(string token);
    Task<(byte[] bytes, string name, string ctype, Guid roomId)> DownloadByTokenAsync(string token);
}
