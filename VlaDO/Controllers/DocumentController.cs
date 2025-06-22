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

[ApiController, Authorize]
[Route("api/rooms/{roomId:guid}/docs")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _docs;
    private readonly IShareService _share;
    private readonly IPermissionService _perm; 
    private readonly IDocumentRepository _docRepo;
    private readonly IUnitOfWork _uow;

    public DocumentController(
    IUnitOfWork uow,
    IDocumentService docService,
    IPermissionService permissionService,
    IShareService shareService)
    {
        _uow = uow;
        _docRepo = _uow.DocumentRepository;
        _docs = docService;
        _perm = permissionService;
        _share = shareService;
    }

    // ──────── ЗАГРУЗКА ───────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Upload(Guid roomId, [FromForm] IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var id = await _docs.UploadAsync(roomId, User.GetUserId(), file.FileName, ms.ToArray());
        return Ok(id);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> UploadMany(Guid roomId, [FromForm] List<IFormFile> files)
    {
        await _docs.UploadManyAsync(roomId, User.GetUserId(), files);
        return Ok();
    }

    // ──────── НОВАЯ ВЕРСИЯ ───────────────────────────────────────
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

    // ──────── СПИСОК / ОДИН ──────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> List(Guid roomId) =>
        Ok(await _docs.ListAsync(roomId, User.GetUserId()));

    [HttpGet("{docId:guid}")]
    public async Task<IActionResult> Download(Guid roomId, Guid docId)
    {
        var (bytes, name, ct) = await _docs.DownloadAsync(docId, User.GetUserId());
        return File(bytes, ct, name);
    }

    // ──────── ZIP НА НЕСКОЛЬКО ───────────────────────────────────
    [HttpPost("archive")]
    public async Task<IActionResult> Zip(Guid roomId, [FromBody] DownloadManyDto dto)
    {
        var (zip, name) = await _docs.DownloadManyAsync(dto.DocumentIds, User.GetUserId());
        return File(zip, "application/zip", name);
    }

    // ──────── ГЕНЕРАЦИЯ / ОТЗЫВ ТОКЕНА ───────────────────────────
    [HttpPost("{docId:guid}/token")]
    public async Task<IActionResult> GenerateToken(Guid roomId, Guid docId,
        [FromBody] GenerateTokenDto dto)
    {
        // только создатель или Manage-доступ
        if (!await _perm.CheckAccessAsync(User.GetUserId(), roomId, AccessLevel.Full))
            return Forbid();

        var token = await _share.ShareDocumentAsync(docId, dto.AccessLevel,
                                                    TimeSpan.FromDays(dto.DaysValid));
        return Ok(new { token });
    }

    [HttpDelete("token/{token}")]
    public async Task<IActionResult> RevokeToken(Guid roomId, string token)
    {
        await _share.RevokeTokenAsync(token);
        return NoContent();
    }

    // ──────── СКАЧИВАНИЕ ПО ТОКЕНУ (тоже Authorize!) ─────────────
    [HttpGet("token/{token}")]
    public async Task<IActionResult> GetByToken(Guid roomId, string token)
    {
        // получаем документ по токену – внутри ShareService проверка срока
        var (bytes, name, ct, docRoomId) = await _share.DownloadByTokenAsync(token);

        // пользователь должен иметь хотя бы Read-доступ к комнате
        if (!await _perm.CheckAccessAsync(User.GetUserId(), docRoomId, AccessLevel.Read))
            return Forbid();

        return File(bytes, ct, name);
    }
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
            Version = 1,
            ForkPath = await DocumentVersionHelper.GenerateInitialForkPathAsync(_uow.Documents, userId)
        };

        if (dto.File != null)
        {
            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);
            newDoc.Data = ms.ToArray();
            newDoc.Hash = ComputeHash(newDoc.Data); // метод, считающий SHA256 или MD5
        }

        await _uow.Documents.AddAsync(newDoc);
        await _uow.CommitAsync();

        return Ok(newDoc.Id);
    }

    private static string ComputeHash(byte[] data)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

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

    [HttpGet("/api/documents/{docId}/download")]
    public async Task<IActionResult> DownloadById(Guid docId)
    {
        var doc = await _uow.Documents.GetByIdAsync(docId);
        if (doc == null || doc.Data == null)
            return NotFound();

        return File(doc.Data, "application/octet-stream", doc.Name);
    }

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
    [HttpDelete("/api/documents/{docId}")]
    public async Task<IActionResult> DeleteDocument(Guid docId)
    {
        var doc = await _uow.Documents.GetByIdAsync(docId);
        if (doc == null) return NotFound();

        var tokens = await _uow.Tokens.FindAsync(t => t.DocumentId == docId);
        foreach (var tok in tokens)
            await _uow.Tokens.DeleteAsync(tok.Id);

        await _uow.Documents.DeleteAsync(docId);

        await _uow.CommitAsync();
        return NoContent();
    }

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
    [HttpPatch("/api/documents/{docId}/rename")]
    public async Task<IActionResult> Rename(Guid docId, [FromBody] RenameDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Новое имя не может быть пустым");

        var userId = User.GetUserId();
        var doc = await _uow.Documents.GetByIdAsync(docId);
        if (doc == null) return NotFound();

        doc.Name = dto.Name.Trim();
        await _uow.CommitAsync();
        return Ok(doc.Id);
    }
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
            (await _uow.Users.GetBriefByIdAsync(doc.CreatedBy))?.Name,
            doc.CreatedBy,
            doc.CreatedOn,
            note,
            doc.ForkPath
        );

        return Ok(dto);
    }
    private async Task<string?> GetNoteAsync(Guid docId)
    {
        var doc = await _uow.Documents.FirstOrDefaultAsync(d => d.Id == docId);
        return doc?.Note;
    }
}
