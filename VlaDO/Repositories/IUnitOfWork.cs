using VlaDO.Models;
using VlaDO.Repositories.Documents;
using VlaDO.Repositories.Rooms;

namespace VlaDO.Repositories
{
    /// <summary>
    /// Представляет единицу работы, инкапсулирующую доступ ко всем репозиториям и контроль сохранения изменений.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Репозиторий для работы с пользователями.
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// Репозиторий для работы с комнатами.
        /// </summary>
        IRoomRepository Rooms { get; }

        /// <summary>
        /// Универсальный репозиторий для работы с документами.
        /// </summary>
        IGenericRepository<Document> Documents { get; }

        /// <summary>
        /// Универсальный репозиторий для токенов доступа к документам.
        /// </summary>
        IGenericRepository<DocumentToken> Tokens { get; }

        /// <summary>
        /// Универсальный репозиторий для пользовательских контактов.
        /// </summary>
        IGenericRepository<UserContact> Contacts { get; }

        /// <summary>
        /// Универсальный репозиторий для связей между пользователями и комнатами.
        /// </summary>
        IGenericRepository<RoomUser> RoomUsers { get; }

        /// <summary>
        /// Универсальный репозиторий для токенов сброса пароля.
        /// </summary>
        IGenericRepository<PasswordResetToken> PasswordResetTokens { get; }

        /// <summary>
        /// Асинхронно сохраняет все изменения, сделанные через репозитории.
        /// </summary>
        /// <returns>Количество затронутых записей.</returns>
        Task<int> CommitAsync();

        /// <summary>
        /// Специализированный репозиторий для работы с документами.
        /// </summary>
        IDocumentRepository DocumentRepository { get; }

        /// <summary>
        /// Универсальный репозиторий для активности пользователя (уведомления и т.д.).
        /// </summary>
        IGenericRepository<Activity> Activities { get; }

        /// <summary>
        /// Асинхронно сохраняет все изменения с возможностью отмены через <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="ct">Токен отмены операции.</param>
        /// <returns>Количество затронутых записей.</returns>
        Task<int> CommitAsync(CancellationToken ct = default);
    }
}
