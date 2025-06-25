using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using VlaDO.DTOs;
using VlaDO.Models;
using VlaDO.Repositories.Documents;

namespace VlaDO.Repositories
{
    /// <summary>
    /// Репозиторий документов с расширенными методами доступа.
    /// </summary>
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        /// <summary>
        /// Конструктор репозитория документов.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        public DocumentRepository(DocumentFlowContext context) : base(context) { }

        /// <summary>
        /// Возвращает все неархивные документы, созданные указанным пользователем.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя (создателя документов).</param>
        /// <returns>Коллекция документов с привязанной комнатой (если есть).</returns>
        public async Task<IEnumerable<Document>> GetByCreatorAsync(Guid userId)
        {
            var query = _context.Documents
                .Where(d => d.CreatedBy == userId)
                .Include(d => d.Room);
                    return await ExcludeArchived(query).ToListAsync();
        }

        /// <summary>
        /// Возвращает все документы, принадлежащие указанной комнате.
        /// </summary>
        /// <param name="roomId">Идентификатор комнаты.</param>
        /// <returns>Коллекция документов, прикреплённых к комнате.</returns>
        public async Task<IEnumerable<Document>> GetByRoomAsync(Guid roomId)
        {
            return await _context.Documents
                .Where(d => d.RoomId == roomId)
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает документы, созданные указанным пользователем в заданной комнате.
        /// </summary>
        /// <param name="roomId">Идентификатор комнаты.</param>
        /// <param name="userId">Идентификатор пользователя (создателя документов).</param>
        /// <returns>Список документов пользователя в указанной комнате.</returns>
        public async Task<IEnumerable<Document>> GetByRoomAndUserAsync(Guid roomId, Guid userId)
        {
            return await _context.Documents
                .Where(d => d.RoomId == roomId && d.CreatedBy == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает документы, созданные пользователем в указанной комнате.
        /// </summary>
        /// <param name="roomId">Идентификатор комнаты.</param>
        /// <param name="userId">Идентификатор пользователя (создателя).</param>
        /// <returns>Коллекция документов, соответствующих критериям.</returns>
        public async Task<IEnumerable<RoomBriefDto>> GetLastActiveRoomsAsync(Guid userId, int top = 10)
        {
            var owned = _context.Rooms
                                .Where(r => r.OwnerId == userId)
                                .Select(r => r.Id);

            var shared = _context.RoomUsers
                                 .Where(ru => ru.UserId == userId)
                                 .Select(ru => ru.RoomId);

            var accessible = await owned.Union(shared).ToListAsync();

            var lastActivity = await _context.Documents
                .Where(d => d.RoomId != null && accessible.Contains(d.RoomId.Value))
                .GroupBy(d => d.RoomId)
                .Select(g => new
                {
                    RoomId = g.Key.Value,
                    Last = g.Max(x => x.CreatedOn)
                })
                .OrderByDescending(x => x.Last)
                .Take(top)
                .ToListAsync();

            var roomIds = lastActivity.Select(x => x.RoomId).ToList();

            var roomTitles = await _context.Rooms
                .Where(r => roomIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Title ?? "-");

            var result = lastActivity
                .Select(x => new RoomBriefDto(
                    x.RoomId,
                    roomTitles.TryGetValue(x.RoomId, out var title) ? title : "-",
                    x.Last));

            return result;
        }

        /// <summary>
        /// Возвращает последние версии документов, доступных пользователю в чужих комнатах или созданных им лично.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Коллекция последних версий документов.</returns>
        public async Task<IEnumerable<Document>> GetByRoomAndUserAsyncExcludeCreator(Guid userId)
        {
            var accessibleRoomIds = await _context.RoomUsers
                .Where(ru => ru.UserId == userId)
                .Select(ru => ru.RoomId)
                .ToListAsync();

            var docs = await _context.Documents
                .Where(d =>
                    d.CreatedBy == userId ||
                    (d.RoomId != null && accessibleRoomIds.Contains(d.RoomId.Value)))
                .ToListAsync();

            var latestDocs = docs
                .GroupBy(d => d.Name)
                .Select(g => g.OrderByDescending(d => d.Version).First())
                .ToList();

            return latestDocs;
        }

        /// <summary>
        /// Возвращает документы пользователя, не прикреплённые ни к одной комнате.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Коллекция документов без комнаты.</returns>
        public async Task<IEnumerable<Document>> GetWithoutRoomAsync(Guid userId)
        {
            return await _context.Documents
                .Where(d => d.CreatedBy == userId && d.RoomId == null)
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает полную цепочку версий документа, начиная с корневого.
        /// </summary>
        /// <param name="docId">Идентификатор документа (любая версия).</param>
        /// <returns>Отсортированная по дате коллекция всех версий документа.</returns>
        public async Task<IEnumerable<Document>> GetVersionChainAsync(Guid docId)
        {
            var root = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == docId);
            
                while (root is { ParentDocId: not null })
                root = await _context.Documents
                                 .AsNoTracking()
                                 .FirstAsync(d => d.Id == root.ParentDocId);
            
                if (root == null)
                        return Enumerable.Empty<Document>();
            var versions = new List<Document> { root };
            var queue = new Queue<Guid>();
            queue.Enqueue(root.Id);

            var candidates = await _context.Documents
                    .AsNoTracking()
                    .Where(d => d.ParentDocId != null)
                    .ToListAsync();
            
                while (queue.Count > 0)
                    {
                var parentId = queue.Dequeue();
                
                        foreach (var child in candidates.Where(c => c.ParentDocId == parentId))
                            {
                    versions.Add(child);
                    queue.Enqueue(child.Id);
                            }
                    }
            
                return versions.OrderBy(d => d.CreatedOn);
        }

        /// <summary>
        /// Возвращает дату последнего изменения (создания документа) в указанной комнате.
        /// </summary>
        /// <param name="roomId">Идентификатор комнаты.</param>
        /// <returns>Дата последнего изменения или null, если документов нет.</returns>
        public async Task<DateTime?> GetLastChangeInRoomAsync(Guid roomId)
        {
            return await _context.Documents
                .Where(d => d.RoomId == roomId)
                .MaxAsync(d => (DateTime?)d.CreatedOn);
        }

        /// <summary>
        /// Возвращает все документы с информацией о связанных комнатах.
        /// </summary>
        /// <returns>Коллекция всех документов с подгруженными комнатами.</returns>
        public async Task<IEnumerable<Document>> GetAllAsyncWithRoomAsync()
        {
            return await _context.Documents
                .Include(d => d.Room)
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает все документы, доступные пользователю:
        /// созданные им, доступные через комнату или через токен доступа.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Коллекция доступных пользователю документов с комнатами.</returns>
        public async Task<IEnumerable<Document>> GetAccessibleToUserAsync(Guid userId)
        {
            var accessibleRoomIds = await _context.RoomUsers
                .Where(ru => ru.UserId == userId)
                .Select(ru => ru.RoomId)
                .ToListAsync();

            var query = _context.Documents
                .Where(doc =>
                    doc.CreatedBy == userId ||
                    (doc.RoomId != null && accessibleRoomIds.Contains(doc.RoomId.Value)) ||
                    doc.Tokens.Any(t => t.UserId == userId)
                )
                .Include(d => d.Room);

            return await ExcludeArchived(query).ToListAsync();
        }

        /// <summary>
        /// Возвращает документы, к которым пользователь имеет доступ,
        /// но не является их создателем и не владеет комнатами, в которых они находятся.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список доступных документов, созданных другими пользователями вне собственных комнат.</returns>
        public async Task<IEnumerable<Document>> GetOtherAccessibleDocsAsync(Guid userId)
        {
            var accessibleDocs = await GetAccessibleToUserAsync(userId);

            var ownRoomIds = await _context.Rooms
                .Where(r => r.OwnerId == userId)
                .Select(r => r.Id)
                .ToListAsync();

            return accessibleDocs
                .Where(d => d.CreatedBy != userId &&
                           (d.RoomId == null || !ownRoomIds.Contains(d.RoomId.Value)))
                .ToList();
        }

        /// <summary>
        /// Возвращает все документы пользователя, находящиеся в комнате с названием "Архив".
        /// </summary>
        /// <param name="userId">Идентификатор пользователя — владельца комнаты "Архив".</param>
        /// <returns>Список архивных документов.</returns>
        public async Task<IEnumerable<Document>> GetArchivedForUserAsync(Guid userId)
        {
            return await _context.Documents
                .Where(d => d.Room != null &&
                            d.Room.Title == "Архив" &&
                            d.Room.OwnerId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Исключает документы, находящиеся в комнате с названием "Архив".
        /// </summary>
        /// <param name="query">Исходный запрос к документам.</param>
        /// <returns>Запрос без архивных документов.</returns>
        private IQueryable<Document> ExcludeArchived(IQueryable<Document> query)
        {
            return query.Where(d => d.Room == null || d.Room.Title != "Архив");
        }

        /// <summary>
        /// Возвращает все версии документов в рамках одной ветки (форка),
        /// начиная с указанного документа.
        /// </summary>
        /// <param name="docId">Идентификатор любого документа из ветки.</param>
        /// <returns>Список документов в форке, включая указанный документ и его потомков.</returns>
        public async Task<List<Document>> GetForkBranchAsync(Guid docId)
        {
            var doc = await _context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == docId);

            if (doc == null)
                return new List<Document>();

            return await _context.Documents
                .Where(d => d.ForkPath.StartsWith(doc.ForkPath))
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает последние (финальные) версии всех доступных пользователю документов,
        /// то есть документы, у которых нет дочерних версий.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список последних доступных пользователю версий документов.</returns>
        public async Task<IEnumerable<Document>> GetLatestVersionsByForkPathAsync(Guid userId)
        {
            var all = await GetAccessibleToUserAsync(userId);

            var lastVersions = all
                .Where(doc => !all.Any(p => p.ParentDocId == doc.Id))
                .ToList();

            return lastVersions;
        }
    }
}