using VlaDO.Models;
using VlaDO.Repositories.Rooms;

namespace VlaDO.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DocumentFlowContext _context;
        public IUserRepository Users { get; }
        public IRoomRepository Rooms { get; }
        public IGenericRepository<Document> Documents { get; }
        public IGenericRepository<DocumentToken> Tokens { get; }
        public IGenericRepository<RoomUser> RoomUsers { get; }

        public UnitOfWork(DocumentFlowContext context)
        {
            _context = context;
            Users = new UserRepository(context);
            Rooms = new RoomRepository(context);
            Documents = new GenericRepository<Document>(context);
            Tokens = new GenericRepository<DocumentToken>(context);
            RoomUsers = new GenericRepository<RoomUser>(context);
        }

        public Task<int> CommitAsync() => _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
