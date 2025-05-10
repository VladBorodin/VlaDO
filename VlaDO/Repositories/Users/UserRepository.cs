using Microsoft.EntityFrameworkCore;
using VlaDO.Models;

namespace VlaDO.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(DocumentFlowContext ctx) : base(ctx) { }

    // ───────── 1. Поиск по e‑mail
    public Task<User?> GetByEmailAsync(string email) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    // ───────── 2. Комнаты, где у пользователя AccessLevel ≥ level
    public async Task<IEnumerable<Room>> GetRoomsWithAccessAsync(Guid userId, AccessLevel level)
    {
        return await _context.RoomUsers
            .Where(ru => ru.UserId == userId && ru.AccessLevel >= level)
            .Include(ru => ru.Room)
            .Select(ru => ru.Room)
            .ToListAsync();
    }

    // ───────── 3. Проверка владельца комнаты
    public Task<bool> IsRoomOwnerAsync(Guid roomId, Guid userId) =>
        _context.Rooms.AnyAsync(r => r.Id == roomId && r.OwnerId == userId);

    // ───────── 4. Все документы, к которым у пользователя есть доступ
    public async Task<IEnumerable<Document>> GetAccessibleDocumentsAsync(Guid userId)
    {
        // a) документы из комнат, где пользователь член
        var roomDocs = await _context.RoomUsers
            .Where(ru => ru.UserId == userId && ru.AccessLevel >= AccessLevel.Read)
            .SelectMany(ru => ru.Room.Documents)
            .ToListAsync();

        // b) документы, расшаренные пользователю по токену (если нужно)
        var tokenDocs = await _context.DocumentTokens
            .Where(dt => dt.ExpiresAt > DateTime.UtcNow &&
                         dt.AccessLevel >= AccessLevel.Read &&
                         dt.Document.CreatedBy == userId)   // или другая логика
            .Select(dt => dt.Document)
            .ToListAsync();

        return roomDocs.Union(tokenDocs).Distinct();
    }
}
