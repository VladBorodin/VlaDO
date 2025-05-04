using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services
{
    public class PermissionService
    {
        private readonly IUnitOfWork _uow;
        public PermissionService(IUnitOfWork uow) => _uow = uow;

        public async Task<bool> CheckAccessAsync(
            Guid userId, Guid docId, AccessLevel required, string? token = null)
        {
            var doc = await _uow.Documents.GetByIdAsync(docId, d => d.Room);

            // 1) владелец документа
            if (doc?.CreatedBy == userId) return true;

            // 2) комната и уровень в RoomUser
            if (doc?.RoomId is Guid roomId)
            {
                var ru = (await _uow.RoomUsers
                            .FindAsync(r => r.RoomId == roomId && r.UserId == userId))
                          .FirstOrDefault();
                if (ru != null && ru.AccessLevel >= required) return true;
            }

            // 3) публичный токен
            if (!string.IsNullOrEmpty(token))
            {
                var dt = (await _uow.Tokens.FindAsync(
                            t => t.Token == token && t.ExpiresAt > DateTime.UtcNow))
                         .FirstOrDefault();
                if (dt != null && dt.DocumentId == docId && dt.AccessLevel >= required)
                    return true;
            }
            return false;
        }
    }
}
