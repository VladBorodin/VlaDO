using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VlaDO.Models;

namespace VlaDO.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(DocumentFlowContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
