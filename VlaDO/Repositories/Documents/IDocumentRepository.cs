using VlaDO.DTOs;
using VlaDO.Models;

namespace VlaDO.Repositories.Documents
{
    /// <summary>
    /// Интерфейс для работы с документами в хранилище.
    /// </summary>
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        /// <summary>Получает документы, созданные указанным пользователем.</summary>
        Task<IEnumerable<Document>> GetByCreatorAsync(Guid userId);

        /// <summary>Получает все документы, привязанные к комнате.</summary>
        Task<IEnumerable<Document>> GetByRoomAsync(Guid roomId);

        /// <summary>Получает документы, созданные пользователем в конкретной комнате.</summary>
        Task<IEnumerable<Document>> GetByRoomAndUserAsync(Guid roomId, Guid userId);

        /// <summary>
        /// Получает документы, доступные пользователю по комнатам, где он участник, 
        /// включая его собственные документы, исключая устаревшие версии.
        /// </summary>
        Task<IEnumerable<Document>> GetByRoomAndUserAsyncExcludeCreator(Guid userId);

        /// <summary>Получает документы пользователя, не привязанные к комнате.</summary>
        Task<IEnumerable<Document>> GetWithoutRoomAsync(Guid userId);

        /// <summary>Получает всю цепочку версий документа, начиная от корня и всех его потомков.</summary>
        Task<IEnumerable<Document>> GetVersionChainAsync(Guid docId);

        /// <summary>Возвращает дату последнего изменения в указанной комнате.</summary>
        Task<DateTime?> GetLastChangeInRoomAsync(Guid roomId);

        /// <summary>Возвращает документы, к которым пользователь имеет доступ напрямую или через комнату.</summary>
        Task<IEnumerable<Document>> GetAccessibleToUserAsync(Guid userId);

        /// <summary>
        /// Получает документы, к которым пользователь имеет доступ, но которые он не создавал 
        /// и которые не принадлежат его комнатам.
        /// </summary>
        Task<IEnumerable<Document>> GetOtherAccessibleDocsAsync(Guid userId);

        /// <summary>Возвращает документы, находящиеся в архивной комнате пользователя.</summary>
        Task<IEnumerable<Document>> GetArchivedForUserAsync(Guid userId);

        /// <summary>Получает все документы в пределах одной ветки (fork) документа.</summary>
        Task<List<Document>> GetForkBranchAsync(Guid docId);

        /// <summary>Возвращает последние версии всех доступных пользователю документов по веткам.</summary>
        Task<IEnumerable<Document>> GetLatestVersionsByForkPathAsync(Guid userId);

        /// <summary>Возвращает последние активные комнаты пользователя по времени изменения документов.</summary>
        Task<IEnumerable<RoomBriefDto>> GetLastActiveRoomsAsync(Guid userId, int top = 3);
    }

}
