using VlaDO.Models;

namespace VlaDO.Repositories.Documents
{
    /// <summary>
    /// Интерфейс для работы с токенами доступа к документам.
    /// </summary>
    public interface IDocumentTokenRepository : IGenericRepository<DocumentToken>
    {
        /// <summary>
        /// Возвращает токен доступа к документу для указанного пользователя, если он существует.
        /// </summary>
        /// <param name="documentId">Идентификатор документа.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Объект <see cref="DocumentToken"/> или null, если не найден.</returns>
        Task<DocumentToken?> GetByDocAndUserAsync(Guid documentId, Guid userId);
    }
}
