using VlaDO.Models;
using VlaDO.Repositories.Rooms;

namespace VlaDO.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoomRepository Rooms { get; }
        IGenericRepository<Document> Documents { get; }
        IGenericRepository<DocumentToken> Tokens { get; }
        Task<int> CommitAsync();
    }
}
