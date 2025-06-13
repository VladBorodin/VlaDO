using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Repositories.Documents;
using VlaDO.Services;

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

        var id = await _docs.UpdateAsync(docId, User.GetUserId(), file);
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

    // ──────── УДАЛЕНИЕ ───────────────────────────────────────────
    [HttpDelete("{docId:guid}")]
    public async Task<IActionResult> Delete(Guid roomId, Guid docId)
    {
        await _docs.DeleteAsync(docId, User.GetUserId());
        return NoContent();
    }

    // ──────── ГЕНЕРАЦИЯ / ОТЗЫВ ТОКЕНА ───────────────────────────
    [HttpPost("{docId:guid}/token")]
    public async Task<IActionResult> GenerateToken(Guid roomId, Guid docId,
        [FromBody] GenerateTokenDto dto)
    {
        // только создатель или Manage-доступ
        if (!await _perm.CheckAccessAsync(User.GetUserId(), roomId, AccessLevel.Admin))
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
            "lastupdate" => await _docRepo.GetLatestVersionsForUserAsync(userId),
            "userDoc" => await _docRepo.GetByCreatorAsync(userId),
            "all" => await _docRepo.GetAccessibleToUserAsync(userId),
            _ => await _docRepo.GetAccessibleToUserAsync(userId)
        };

        var result = new List<DocumentDto>();

        foreach (var doc in documents)
        {
            var user = await _uow.Users.GetBriefByIdAsync(doc.CreatedBy);

            RoomBriefDto? roomDto = null;
            string accessLevel = "Read";

            // ─── Определение доступа ───────────────────────────────
            if (doc.RoomId is Guid roomId)
            {
                var room = doc.Room ?? await _uow.Rooms.GetByIdAsync(roomId);
                var lastChange = await _docRepo.GetLastChangeInRoomAsync(roomId);
                roomDto = new RoomBriefDto(room.Id, room.Title, lastChange);

                accessLevel = (await _perm.GetAccessLevelAsync(userId, roomId)).ToString();
            }
            else if (doc.CreatedBy == userId)
            {
                accessLevel = "Full";
            }
            else
            {
                var token = await _uow.Tokens.FirstOrDefaultAsync(t => t.DocumentId == doc.Id && t.UserId == userId);
                if (token != null)
                    accessLevel = token.AccessLevel.ToString();
            }

            result.Add(new DocumentDto
            {
                Id = doc.Id,
                Name = doc.Name,
                Version = doc.Version,
                CreatedAt = doc.CreatedOn,
                CreatedBy = user!,
                Room = roomDto,
                PreviousVersionId = doc.ParentDocId,
                AccessLevel = accessLevel
            });
        }

        return Ok(result);
    }

    [HttpGet("/api/documents/{docId}/versions")]
    public async Task<IActionResult> GetVersions(Guid docId)
    {
        var userId = User.GetUserId();

        // 1. Получаем документ, чтобы проверить доступ
        var doc = await _docRepo.GetByIdAsync(docId);
        if (doc == null)
            return NotFound();

        // 2. Проверка доступа
        if (doc.RoomId != null)
        {
            var hasAccess = await _perm.CheckAccessAsync(userId, doc.RoomId.Value, AccessLevel.Read);
            if (!hasAccess)
                return Forbid();
        }
        else if (doc.CreatedBy != userId)
        {
            return Forbid();
        }

        // 3. Получаем цепочку версий
        var versions = await _docRepo.GetVersionChainAsync(docId);

        var dtoList = versions.Select(d => new DocumentInfoDto(
            d.Id,
            d.Name,
            d.Version,
            d.ParentDocId,
            d.Hash,
            d.PrevHash,
            d.CreatedOn,
            d.Note
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
            Version = 1 // потом можем апдейтить
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
    [HttpPut("/api/documents/{docId}")]
    public async Task<IActionResult> UpdateDocument(Guid docId, [FromForm] UpdateDocumentDto dto)
    {
        var userId = User.GetUserId();

        var doc = await _docRepo.GetByIdAsync(docId);
        if (doc == null)
            return NotFound();

        if (doc.RoomId != null)
        {
            var hasAccess = await _perm.CheckAccessAsync(userId, doc.RoomId.Value, AccessLevel.Edit);
            if (!hasAccess) return Forbid();
        }
        else if (doc.CreatedBy != userId)
        {
            return Forbid();
        }

        if (dto.Name != null) doc.Name = dto.Name;
        if (dto.Note != null) doc.Note = dto.Note;
        if (dto.RoomId != null) doc.RoomId = dto.RoomId;
        if (dto.ParentDocId != null) doc.ParentDocId = dto.ParentDocId;

        if (dto.File != null)
        {
            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);
            doc.Data = ms.ToArray();
            doc.PrevHash = doc.Hash;
            doc.Hash = ComputeHash(doc.Data);
            doc.Version += 1;
        }

        await _uow.CommitAsync();
        return Ok(doc.Id);
    }
}
