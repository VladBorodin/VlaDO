using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers;

[ApiController, Authorize]
[Route("api/documents/{docId:guid}")]
public class DocumentTokenController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _perm;
    public DocumentTokenController(IUnitOfWork uow, IPermissionService perm)
    {
        _uow = uow;
        _perm = perm;
    }

    // ───────── список всех «шаров» ─────────
    [HttpGet("tokens")]
    public async Task<IActionResult> List(Guid docId)
    {
        // Только создатель или Full-доступ
        if (!await _perm.CheckAccessAsync(User.GetUserId(), docId, AccessLevel.Full))
            return Forbid();

        var tokens = await _uow.Tokens
            .FindAsync(t => t.DocumentId == docId && t.UserId != Guid.Empty);

        var uids = tokens.Select(t => t.UserId).Distinct().ToArray();
        var users = await _uow.Users.FindAsync(u => uids.Contains(u.Id));

        var result = tokens.Select(t => new DocumentShareDto(
            t.Id,
            t.UserId,
            users.First(u => u.Id == t.UserId).Name,
            t.AccessLevel)).ToArray();

        return Ok(result);
    }

    // ───────── создать / обновить доступ ─────────
    [HttpPost("token")]
    public async Task<IActionResult> Upsert(Guid docId, [FromBody] UpdateAccessDto dto)
    {
        if (!await _perm.CheckAccessAsync(User.GetUserId(), docId, AccessLevel.Full))
            return Forbid();

        var tok = (await _uow.Tokens
            .FindAsync(t => t.DocumentId == docId && t.UserId == dto.UserId))
            .FirstOrDefault();

        if (tok == null)
        {
            tok = new DocumentToken
            {
                DocumentId = docId,
                UserId = dto.UserId,
                Token = Guid.NewGuid().ToString("N"),
                AccessLevel = dto.AccessLevel,
                ExpiresAt = DateTime.UtcNow.AddYears(5)
            };
            await _uow.Tokens.AddAsync(tok);
        }
        else
        {
            tok.AccessLevel = dto.AccessLevel;
        }

        await _uow.CommitAsync();
        return Ok();
    }

    // ───────── отозвать доступ ─────────
    [HttpDelete("token/{tokenId:guid}")]
    public async Task<IActionResult> Delete(Guid docId, Guid tokenId)
    {
        if (!await _perm.CheckAccessAsync(User.GetUserId(), docId, AccessLevel.Full))
            return Forbid();

        await _uow.Tokens.DeleteAsync(tokenId);
        await _uow.CommitAsync();
        return NoContent();
    }
    [HttpGet("shared-users")]
    public async Task<IActionResult> SharedUsers(Guid docId)
    {
        var shares = await _uow.Tokens.FindAsync(
                        t => t.DocumentId == docId && t.UserId != Guid.Empty,
                        include: t => t.User);

        var dto = shares.Select(s => new UserBriefDto(
                         s.UserId,
                         s.User!.Name));

        return Ok(dto);
    }
}