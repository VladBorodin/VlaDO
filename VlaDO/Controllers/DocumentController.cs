using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Services;

namespace VlaDO.Controllers;

[ApiController, Authorize]
[Route("api/rooms/{roomId:guid}/docs")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _docs;
    private readonly IShareService _share;
    private readonly IPermissionService _perm;

    public DocumentController(
        IDocumentService docs,
        IShareService share,
        IPermissionService perm)
    {
        _docs = docs;
        _share = share;
        _perm = perm;
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
        if (!await _perm.CheckAccessAsync(User.GetUserId(), roomId, AccessLevel.Manage))
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
}
