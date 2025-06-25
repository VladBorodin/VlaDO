using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Repositories.Documents;
using VlaDO.Services;
using VlaDO.Helpers;

namespace VlaDO.Controllers;

/// <summary>
/// Работа с документами внутри комнат и вне их (загрузка, версии, токены, архив).
/// </summary>
[ApiController, Authorize]
[Route("api/rooms/{roomId:guid}/docs")]
public class DocumentController : ControllerBase
{
    /// <summary>Сервис документов.</summary>
    private readonly IDocumentService _docs;
    /// <summary>Сервис расшаривания документов по токену.</summary>
    private readonly IShareService _share;
    /// <summary>Сервис проверки прав доступа.</summary>
    private readonly IPermissionService _perm;
    /// <summary>Репозиторий документов.</summary>
    private readonly IDocumentRepository _docRepo;
    /// <summary>Unit-of-Work для операций БД.</summary>
    private readonly IUnitOfWork _uow;
    /// <summary>Логгер пользовательской активности.</summary>
    private readonly IActivityLogger _logger;

    /// <summary>
    /// Контроллер для управления документами. Включает сервисы доступа, логирования и репозиторий.
    /// </summary>
    /// <param name="uow">Единица работы для доступа к репозиториям.</param>
    /// <param name="docService">Сервис обработки документов.</param>
    /// <param name="permissionService">Сервис проверки прав доступа.</param>
    /// <param name="shareService">Сервис шаринга документов по токену.</param>
    /// <param name="logger">Сервис логирования активности.</param>
    public DocumentController(IUnitOfWork uow, IDocumentService docService, IPermissionService permissionService,
        IShareService shareService, IActivityLogger logger)
    {
        _uow = uow;
        _docRepo = _uow.DocumentRepository;
        _docs = docService;
        _perm = permissionService;
        _share = shareService;
        _logger = logger;
    }

    // ──────────────────────────── Uploads ───────────────────────────

    /// <summary>
    /// Загружает новый документ в указанную комнату.
    /// </summary>
    /// <param name="roomId">Комната-приёмник.</param>
    /// <param name="file">Файл, отправленный в multipart/form-data.</param>
    /// <returns>ID созданного документа.</returns>
    [HttpPost]
    public async Task<IActionResult> Upload(Guid roomId, [FromForm] IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var id = await _docs.UploadAsync(roomId, User.GetUserId(), file.FileName, ms.ToArray());
        return Ok(id);
    }

    /// <summary>
    /// Массовая загрузка файлов в комнату.
    /// </summary>
    /// <param name="roomId">Комната-приёмник.</param>
    /// <param name="files">Список файлов.</param>
    [HttpPost("bulk")]
    public async Task<IActionResult> UploadMany(Guid roomId, [FromForm] List<IFormFile> files)
    {
        await _docs.UploadManyAsync(roomId, User.GetUserId(), files);
        return Ok();
    }

    /// <summary>
    /// Добавляет новую версию существующего документа.
    /// </summary>
    /// <param name="roomId">Комната, к которой относится документ.</param>
    /// <param name="docId">Идентификатор документа.</param>
    /// <param name="file">Файл новой версии.</param>
    /// <returns>ID созданной версии.</returns>
    [HttpPost("{docId:guid}/version")]
    public async Task<IActionResult> NewVersion(Guid roomId, Guid docId, IFormFile file)
    {
        var userId = User.GetUserId();
        var hasAccess = await _perm.CheckRoomAccessAsync(userId, roomId, AccessLevel.Edit);
        if (!hasAccess)
            return Forbid();

        var id = await _docs.UpdateAsync(roomId, docId, User.GetUserId(), file);
        return Ok(id);
    }

    /// <summary>
    /// Возвращает список документов в комнате, доступных пользователю.
    /// </summary>
    /// <param name="roomId">Комната, к которой относится документ.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> List(Guid roomId) =>
        Ok(await _docs.ListAsync(roomId, User.GetUserId()));

    /// <summary>
    /// Возвращает список документов в комнате, доступных пользователю.
    /// </summary>
    /// <param name="roomId">Комната, к которой относится документ.</param>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>Список документов</returns>
    [HttpGet("{docId:guid}")]
    public async Task<IActionResult> Download(Guid roomId, Guid docId)
    {
        var (bytes, name, ct) = await _docs.DownloadAsync(docId, User.GetUserId());
        return File(bytes, ct, name);
    }

    /// <summary>
    /// Скачивает указанные документы в одном ZIP-архиве.
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты, в рамках которой выполняется операция. Используется для маршрутизации, но не влияет на выбор документов.</param>
    /// <param name="dto">Объект, содержащий список идентификаторов документов для архивации.</param>
    /// <returns>ZIP-файл с выбранными документами в виде бинарного потока.</returns>
    [HttpPost("archive")]
    public async Task<IActionResult> Zip(Guid roomId, [FromBody] DownloadManyDto dto)
    {
        var (zip, name) = await _docs.DownloadManyAsync(dto.DocumentIds, User.GetUserId());
        return File(zip, "application/zip", name);
    }

    /// <summary>
    /// Генерирует временный токен доступа к документу с заданным уровнем прав.
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты, в рамках которой проверяется доступ пользователя.</param>
    /// <param name="docId">Идентификатор документа, для которого создаётся токен.</param>
    /// <param name="dto">Данные для генерации токена: уровень доступа и срок действия.</param>
    /// <returns>Объект с созданным токеном доступа.</returns>
    [HttpPost("{docId:guid}/token")]
    public async Task<IActionResult> GenerateToken(Guid roomId, Guid docId,
        [FromBody] GenerateTokenDto dto)
    {
        if (!await _perm.CheckAccessAsync(User.GetUserId(), roomId, AccessLevel.Full))
            return Forbid();

        var token = await _share.ShareDocumentAsync(docId, dto.AccessLevel,
                                                    TimeSpan.FromDays(dto.DaysValid));
        return Ok(new { token });
    }

    /// <summary>
    /// Отзывает ранее сгенерированный токен доступа к документу.
    /// </summary>
    /// <param name="roomId">Комната, в рамках которой производится проверка прав пользователя.</param>
    /// <param name="token">Строковое представление токена, подлежащего отзыву.</param>
    /// <returns>Результат выполнения операции.</returns>
    [HttpDelete("token/{token}")]
    public async Task<IActionResult> RevokeToken(Guid roomId, string token)
    {
        await _share.RevokeTokenAsync(token);
        return NoContent();
    }

    /// <summary>
    /// Загружает документ по предоставленному токену доступа.
    /// </summary>
    /// <param name="roomId">Комната, для которой проверяется доступ (может отличаться от фактической комнаты документа).</param>
    /// <param name="token">Токен временного доступа к документу.</param>
    /// <returns>Файл документа, если доступ разрешён.</returns>
    [HttpGet("token/{token}")]
    public async Task<IActionResult> GetByToken(Guid roomId, string token)
    {
        var (bytes, name, ct, docRoomId) = await _share.DownloadByTokenAsync(token);

        if (!await _perm.CheckAccessAsync(User.GetUserId(), docRoomId, AccessLevel.Read))
            return Forbid();

        return File(bytes, ct, name);
    }

    /// <summary>
    /// Возвращает список документов, доступных пользователю, с фильтрацией по типу.
    /// </summary>
    /// <param name="type">
    /// Тип выборки:
    /// <br/>- "own" — документы, созданные пользователем;
    /// <br/>- "otherDoc" — чужие документы с доступом;
    /// <br/>- "lastupdate" — последние версии документов по ForkPath;
    /// <br/>- "userDoc" — синоним "own";
    /// <br/>- "all" — все доступные документы;
    /// <br/>- "archived" — перемещённые в архив;
    /// <br/>- null или неизвестное значение — по умолчанию все доступные.
    /// </param>
    /// <returns>Список документов в расширенном формате.</returns>
    [HttpGet("/api/documents")]
    public async Task<IActionResult> GetDocuments([FromQuery] string? type)
    {
        var userId = User.GetUserId();
        IEnumerable<Document> documents = type switch
        {
            "own" => await _docRepo.GetByCreatorAsync(userId),
            "otherDoc" => await _docRepo.GetOtherAccessibleDocsAsync(userId),
            "lastupdate" => await _docRepo.GetLatestVersionsByForkPathAsync(userId),
            "userDoc" => await _docRepo.GetByCreatorAsync(userId),
            "all" => await _docRepo.GetAccessibleToUserAsync(userId),
            "archived" => await _docRepo.GetArchivedForUserAsync(userId),
            _ => await _docRepo.GetAccessibleToUserAsync(userId)
        };

        var result = new List<DocumentDto>();

        foreach (var doc in documents)
        {
            var user = await _uow.Users.GetBriefByIdAsync(doc.CreatedBy);

            RoomBriefDto? roomDto = null;
            string accessLevel;

            if (doc.RoomId is Guid roomId)
            {
                var room = doc.Room ?? await _uow.Rooms.GetByIdAsync(roomId);
                var lastChange = await _docRepo.GetLastChangeInRoomAsync(roomId);
                roomDto = new RoomBriefDto(room.Id, room.Title, lastChange);
            }

            if (doc.CreatedBy == userId)
            {
                accessLevel = "Full";
            }
            else if (doc.RoomId is Guid roomId2)
            {
                accessLevel = (await _perm
                    .GetAccessLevelAsync(userId, roomId2)).ToString();
            }
            else
            {
                var tok = await _uow.Tokens
                    .FirstOrDefaultAsync(t => t.DocumentId == doc.Id && t.UserId == userId);

                accessLevel = tok?.AccessLevel.ToString() ?? "Read";
            }

            // ───── Сборка DTO ─────
            result.Add(new DocumentDto
            {
                Id = doc.Id,
                Name = doc.Name,
                Version = doc.Version,
                CreatedAt = doc.CreatedOn,
                CreatedBy = user!,
                Room = roomDto,
                PreviousVersionId = doc.ParentDocId,
                AccessLevel = accessLevel,
                ForkPath = doc.ForkPath
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Возвращает все версии указанного документа в порядке цепочки версий.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>Список версий документа с технической информацией.</returns>
    [HttpGet("/api/documents/{docId}/versions")]
    public async Task<IActionResult> GetVersions(Guid docId)
    {
        var userId = User.GetUserId();

        var doc = await _docRepo.GetByIdAsync(docId);
        if (doc == null)
            return NotFound();

        var versions = await _docRepo.GetVersionChainAsync(docId);

        var dtoList = versions.Select(d => new DocumentInfoDto(
            d.Id,
            d.Name,
            d.Version,
            d.ParentDocId,
            d.Hash,
            d.PrevHash,
            d.CreatedOn,
            d.Note,
            d.ForkPath
        ));

        return Ok(dtoList);
    }

    /// <summary>
    /// Создаёт новый документ, при необходимости связывая его с предыдущей версией.
    /// </summary>
    /// <param name="dto">Данные для создания документа, включая файл, заметку и привязку к комнате.</param>
    /// <returns>Идентификатор созданного документа.</returns>
    [HttpPost("create")]
    public async Task<IActionResult> CreateDocument([FromForm] CreateDocumentDto dto)
    {
        var userId = User.GetUserId();

        var newDoc = new Document
        {
            Name = dto.Name,
            CreatedBy = userId,
            CreatedOn = DateTime.UtcNow,
            Note = dto.Note,
            RoomId = dto.RoomId,
            Version = 1
        };

        if (dto.File != null)
        {
            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);
            newDoc.Data = ms.ToArray();
            newDoc.Hash = ComputeHash(newDoc.Data);
        }

        if (!string.IsNullOrWhiteSpace(dto.PrevHash))
        {
            var parent = await _uow.Documents.FirstOrDefaultAsync(d =>
                d.Hash == dto.PrevHash &&
                d.RoomId == dto.RoomId);

            if (parent != null)
            {
                newDoc.ParentDocId = parent.Id;
                newDoc.PrevHash = parent.Hash;
                (newDoc.Version, newDoc.ForkPath) = await DocumentVersionHelper.GenerateNextVersionAsync(_uow.Documents, parent);
            }
        }

        if (string.IsNullOrEmpty(newDoc.ForkPath))
        {
            newDoc.Version = 1;
            newDoc.ForkPath = await DocumentVersionHelper.SafeGenerateInitialForkPathAsync(_uow.Documents, userId, dto.RoomId ?? Guid.Empty);
        }

        await _uow.Documents.AddAsync(newDoc);
        await _uow.CommitAsync();

        return Ok(newDoc.Id);
    }

    /// <summary>
    /// Создаёт новую версию существующего документа, добавляя файл и примечание.
    /// </summary>
    /// <param name="roomId">Комната, к которой относится документ.</param>
    /// <param name="docId">Идентификатор оригинального документа, на основе которого создаётся новая версия.</param>
    /// <param name="dto">DTO с новым файлом и необязательным примечанием.</param>
    /// <returns>Идентификатор новой версии документа.</returns>
    [HttpPost("{docId:guid}/new-version")]
    public async Task<IActionResult> CreateNewVersion(Guid roomId, Guid docId,[FromForm] UpdateDocumentDto dto)
    {
        if (dto.File is null)
            return BadRequest("File missing");
    
        var newId = await _docs.UpdateAsync(
            roomId,
            docId,
            User.GetUserId(),
            dto.File,
            dto.Note);
    
        return Ok(newId);
    }

    /// <summary>
    /// Загружает содержимое документа по его идентификатору.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>Файл документа или 404, если не найден.</returns>
    [HttpGet("/api/documents/{docId}/download")]
    public async Task<IActionResult> DownloadById(Guid docId)
    {
        var doc = await _uow.Documents.GetByIdAsync(docId);
        if (doc == null || doc.Data == null)
            return NotFound();

        return File(doc.Data, "application/octet-stream", doc.Name);
    }

    /// <summary>
    /// Загружает выбранные документы одним ZIP-архивом.
    /// </summary>
    /// <param name="dto">Список идентификаторов документов.</param>
    /// <returns>ZIP-архив с доступными документами или 403, если доступ запрещён.</returns>
    [HttpPost("/api/documents/archive")]
    public async Task<IActionResult> DownloadArchive([FromBody] DownloadManyDto dto)
    {
        var userId = User.GetUserId();

        var documents = new List<Document>();
        foreach (var id in dto.DocumentIds)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc == null || doc.Data == null) continue;

            bool hasAccess = doc.RoomId != null
                ? await _perm.CheckAccessAsync(userId, doc.RoomId.Value, AccessLevel.Read)
                : doc.CreatedBy == userId || await _uow.Tokens.AnyAsync(t => t.DocumentId == id && t.UserId == userId);

            if (hasAccess)
                documents.Add(doc);
        }

        if (documents.Count == 0)
            return Forbid();

        using var archiveStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(archiveStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            foreach (var doc in documents)
            {
                var entry = archive.CreateEntry(doc.Name ?? "unknown", System.IO.Compression.CompressionLevel.Fastest);
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(doc.Data!);
            }
        }

        archiveStream.Seek(0, SeekOrigin.Begin);
        return File(archiveStream.ToArray(), "application/zip", "documents.zip");
    }

    /// <summary>
    /// Удаляет документ по идентификатору.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>204 No Content при успешном удалении.</returns>
    [HttpDelete("/api/documents/{docId}")]
    public async Task<IActionResult> DeleteDocument(Guid docId)
    {
        var userId = User.GetUserId();
        await _docs.DeleteAsync(docId, userId);
        return NoContent();
    }

    /// <summary>
    /// Удаляет все документы в указанной комнате (Room), если у пользователя есть полный доступ.
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты.</param>
    /// <returns>204 No Content при успешном удалении или если документов не было.</returns>
    [HttpDelete("/api/rooms/{roomId:guid}/documents")]
    public async Task<IActionResult> DeleteAllInRoom(Guid roomId)
    {
        var userId = User.GetUserId();

        var isFull = await _perm.CheckAccessAsync(userId, roomId, AccessLevel.Full);
        if (!isFull) return Forbid();

        var docsInRoom = await _uow.DocumentRepository.GetByRoomAsync(roomId);

        if (!docsInRoom.Any()) return NoContent();

        foreach (var doc in docsInRoom)
        {
            await _uow.Documents.DeleteAsync(doc.Id);
            var tokens = await _uow.Tokens.FindAsync(t => t.DocumentId == doc.Id);
            foreach (var tok in tokens)
                await _uow.Tokens.DeleteAsync(tok.Id);
        }

        await _uow.CommitAsync();

        return NoContent();
    }

    /// <summary>
    /// Переименовывает документ по указанному идентификатору.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <param name="dto">DTO с новым именем документа.</param>
    /// <returns>Идентификатор обновлённого документа.</returns>
    [HttpPatch("/api/documents/{docId}/rename")]
    public async Task<IActionResult> Rename(Guid docId, [FromBody] RenameDto dto)
    {
        var userId = User.GetUserId();
        var id = await _docs.RenameAsync(docId, userId, dto.Name);
        return Ok(id);
    }

    /// <summary>
    /// Создаёт копию документа в текущей или указанной комнате.
    /// </summary>
    /// <param name="docId">Идентификатор исходного документа.</param>
    /// <param name="dto">DTO с целевой комнатой для копии.</param>
    /// <returns>Идентификатор нового скопированного документа.</returns>
    [HttpPost("/api/documents/{docId}/copy")]
    public async Task<IActionResult> CopyDocument(Guid docId, [FromBody] CopyDocumentDto dto)
    {
        var userId = User.GetUserId();
        var original = await _uow.Documents.GetByIdAsync(docId);
        if (original == null) return NotFound();

        var targetRoomId = dto.TargetRoomId;
        if (targetRoomId != null)
        {
            var access = await _perm.CheckRoomAccessAsync(userId, targetRoomId.Value, AccessLevel.Edit);
            if (!access)
                return Forbid();
        }

        var baseName = original.Name ?? "Копия";
        var name = baseName;
        int index = 1;
        var existingNames = await _uow.Documents
            .FindAsync(d => d.CreatedBy == userId && d.RoomId == targetRoomId);

        while (existingNames.Any(d => d.Name == name))
        {
            name = $"{baseName} ({index++})";
        }

        var copy = new Document
        {
            Name = name,
            CreatedBy = userId,
            CreatedOn = DateTime.UtcNow,
            RoomId = targetRoomId,
            Version = 1,
            Note = original.Note,
            Data = original.Data,
            Hash = original.Hash
        };

        await _uow.Documents.AddAsync(copy);
        await _uow.CommitAsync();

        return Ok(copy.Id);
    }

    /// <summary>
    /// Удаляет привязку документа к комнате, если у пользователя есть права редактирования.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>Результат операции.</returns>
    [HttpPost("/api/documents/{docId:guid}/remove-room")]
    public async Task<IActionResult> RemoveFromRoom(Guid docId)
    {
        var userId = User.GetUserId();

        var doc = await _uow.Documents.GetByIdAsync(docId);
        if (doc is null || doc.RoomId is null)
            return NotFound();

        var canEdit = await _perm.CheckRoomAccessAsync(
                          userId, doc.RoomId.Value, AccessLevel.Edit);
        if (!canEdit) return Forbid();

        doc.RoomId = null;

        await _uow.CommitAsync();
        return Ok();
    }

    /// <summary>
    /// Добавляет документ в указанную комнату, если у пользователя есть права на редактирование.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <param name="roomId">Комната, к которой относится документ.</param>
    /// <returns>Результат операции.</returns>
    [HttpPost("/api/documents/{docId:guid}/add-to-room/{roomId:guid}")]
    public async Task<IActionResult> AddToRoom(Guid docId, Guid roomId)
    {
        var userId = User.GetUserId();

        var canEdit = await _perm.CheckRoomAccessAsync(
                          userId, roomId, AccessLevel.Edit);
        if (!canEdit) return Forbid();

        var doc = await _uow.Documents.GetByIdAsync(docId);
        if (doc is null) return NotFound();

        doc.RoomId = roomId;
        await _uow.CommitAsync();
        return Ok();
    }

    /// <summary>
    /// Возвращает метаданные документа, включая имя, размер, автора, дату создания и другие свойства.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>Метаданные документа в формате <see cref="DocumentMetaDto"/> или код ошибки.</returns>
    [HttpGet("/api/documents/{docId:guid}/meta")]
    public async Task<IActionResult> GetMeta(Guid docId)
    {
        var doc = await _uow.Documents.GetByIdAsync(docId);
        if (doc is null) return NotFound();

        var note = await GetNoteAsync(docId);
        var uid = User.GetUserId();
        var allowed = await _perm.CheckAccessAsync(uid, docId, AccessLevel.Read);
        if (!allowed) return Forbid();

        var dto = new DocumentMetaDto(
            doc.Id,
            doc.Name,
            doc.Version,
            doc.Data?.LongLength ?? 0,
            Path.GetExtension(doc.Name).Trim('.').ToLowerInvariant(),
            doc.Room?.Title,
            doc.RoomId,
            (await _uow.Users.GetBriefByIdAsync(doc.CreatedBy))?.Name,
            doc.CreatedBy,
            doc.CreatedOn,
            note,
            doc.ForkPath
        );

        return Ok(dto);
    }

    /// <summary>
    /// Получает примечание (Note) к документу по его идентификатору.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>Примечание к документу или null, если документ не найден.</returns>
    private async Task<string?> GetNoteAsync(Guid docId)
    {
        var doc = await _uow.Documents.FirstOrDefaultAsync(d => d.Id == docId);
        return doc?.Note;
    }

    /// <summary>
    /// Очищает архив пользователя, удаляя все документы из комнаты с названием "Архив".
    /// </summary>
    /// <returns>HTTP 204 No Content, если архив пуст или успешно очищен.</returns>
    [HttpDelete("~/api/documents/archived")]
    public async Task<IActionResult> ClearArchive()
    {
        var userId = User.GetUserId();
        var archiveRoom = await _uow.Rooms.FirstOrDefaultAsync(r => r.OwnerId == userId && r.Title == "Архив");
        if (archiveRoom == null)
            return NoContent();

        var docs = await _uow.Documents.FindAsync(d => d.RoomId == archiveRoom.Id && d.CreatedBy == userId);
        foreach (var d in docs)
        {
            var toks = await _uow.Tokens.FindAsync(t => t.DocumentId == d.Id);
            await _uow.Tokens.DeleteRangeAsync(toks);
            await _uow.Documents.DeleteAsync(d);
        }

        await _uow.CommitAsync();
        return NoContent();
    }

    /// <summary>
    /// Восстанавливает цепочку версий документа из архива в указанную комнату.
    /// </summary>
    /// <param name="docId">Идентификатор документа для восстановления.</param>
    /// <param name="dto">Объект с целевым идентификатором комнаты.</param>
    /// <returns>HTTP 204 No Content при успехе, или ошибка доступа/не найдено.</returns>
    [HttpPatch("~/api/documents/{docId:guid}/unarchive")]
    public async Task<IActionResult> Unarchive(Guid docId, [FromBody] UnarchiveDto dto)
    {
        var userId = User.GetUserId();

        var ids = await _uow.DocumentRepository
                             .GetVersionChainAsync(docId)
                             .ContinueWith(t => t.Result.Select(d => d.Id));

        if (!ids.Any()) return NotFound();

        if (dto.TargetRoomId is Guid roomId &&
            !await _perm.CheckRoomAccessAsync(userId, roomId, AccessLevel.Edit))
            return Forbid();

        foreach (var id in ids)
        {
            var doc = await _uow.Documents.GetByIdAsync(id);
            if (doc != null) doc.RoomId = dto.TargetRoomId;
        }

        await _uow.CommitAsync();
        return NoContent();
    }

    /// <summary>
    /// Получает последние добавленные пользователем документы, отсортированные по дате.
    /// </summary>
    /// <param name="top">Количество документов для возврата. По умолчанию 10.</param>
    /// <returns>Список последних документов с датой создания и названием комнаты.</returns>
    [HttpGet("/api/documents/latest")]
    public async Task<IActionResult> Latest([FromQuery] int top = 10)
    {
        var uid = User.GetUserId();
        var docs = await _docRepo.GetLatestVersionsByForkPathAsync(uid);

        var dto = docs
            .OrderByDescending(d => d.CreatedOn)
            .Take(top)
            .Select(d => new {
                d.Id,
                d.Name,
                d.CreatedOn,
                Room = d.Room?.Title
            });

        return Ok(dto);
    }

    /// <summary>
    /// Вычисляет SHA-256 хеш для переданных бинарных данных.
    /// </summary>
    /// <param name="data">Массив байтов, для которого требуется вычислить хеш.</param>
    /// <returns>Строка хеша в нижнем регистре без разделителей.</returns>
    private static string ComputeHash(byte[] data)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
    }
}
