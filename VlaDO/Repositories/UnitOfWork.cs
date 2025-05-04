using VlaDO.Models;
using VlaDO.Repositories.Rooms;

namespace VlaDO.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DocumentFlowContext _ctx;
        public IUserRepository Users { get; }
        public IRoomRepository Rooms { get; }
        public IGenericRepository<Document> Documents { get; }
        public IGenericRepository<DocumentToken> Tokens { get; }
        public IGenericRepository<RoomUser> RoomUsers { get; }

        public UnitOfWork(DocumentFlowContext ctx)
        {
            _ctx = ctx;
            Users = new UserRepository(ctx);
            Rooms = new RoomRepository(ctx);
            Documents = new GenericRepository<Document>(ctx);
            Tokens = new GenericRepository<DocumentToken>(ctx);
            RoomUsers = new GenericRepository<RoomUser>(ctx);
        }

        public Task<int> CommitAsync() => _ctx.SaveChangesAsync();

        public void Dispose() => _ctx.Dispose();
    }
}
