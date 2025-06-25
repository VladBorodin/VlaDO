using Microsoft.EntityFrameworkCore;
using VlaDO.Models;

namespace VlaDO.Repositories.Documents
{
    /// <summary>
    /// Репозиторий для работы с токенами доступа к документам.
    /// </summary>
    public class DocumentTokenRepository : GenericRepository<DocumentToken>, IDocumentTokenRepository
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="DocumentTokenRepository"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        public DocumentTokenRepository(DocumentFlowContext context) : base(context)
        {
        }

        /// <summary>
        /// Возвращает токен доступа пользователя к конкретному документу.
        /// </summary>
        /// <param name="documentId">Идентификатор документа.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Объект <see cref="DocumentToken"/>, если найден; иначе — null.</returns>
        public async Task<DocumentToken?> GetByDocAndUserAsync(Guid documentId, Guid userId)
        {
            return await _context.DocumentTokens
                .FirstOrDefaultAsync(dt => dt.DocumentId == documentId && dt.UserId == userId);
        }
    }
}
