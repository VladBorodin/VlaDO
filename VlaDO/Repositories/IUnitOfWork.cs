using VlaDO.Models;
using VlaDO.Repositories.Documents;
using VlaDO.Repositories.Rooms;

namespace VlaDO.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoomRepository Rooms { get; }
        IGenericRepository<Document> Documents { get; }
        IGenericRepository<DocumentToken> Tokens { get; }
        IGenericRepository<UserContact> Contacts { get; }
        IGenericRepository<RoomUser> RoomUsers { get; }
        IGenericRepository<PasswordResetToken> PasswordResetTokens { get; }
        Task<int> CommitAsync(); 
        IDocumentRepository DocumentRepository { get; }
        IGenericRepository<Activity> Activities { get; }
        Task<int> CommitAsync(CancellationToken ct = default);
    }
}
