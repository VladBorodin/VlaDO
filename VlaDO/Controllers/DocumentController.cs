using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers;

[ApiController, Route("api/documents")]
public class DocumentController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public DocumentController(IUnitOfWork u) => _uow = u;

    [Authorize]
    [HttpPost("{id:guid}/tokens")]
    public async Task<IActionResult> GenerateToken(Guid id, [FromBody] GenerateTokenDto dto)
    {
        var userId = User.GetUserId();
        var doc = await _uow.Documents.GetByIdAsync(id);
        if (doc?.CreatedBy != userId) return Forbid();

        var token = new DocumentToken
        {
            DocumentId = id,
            Token = Guid.NewGuid().ToString("N"),
            AccessLevel = dto.AccessLevel,
            ExpiresAt = DateTime.UtcNow.AddDays(dto.DaysValid)
        };
        await _uow.Tokens.AddAsync(token);
        await _uow.CommitAsync();
        return Ok(new { token = token.Token });
    }

    [AllowAnonymous]
    [HttpGet("shared/{token}")]
    public async Task<IActionResult> GetByToken(string token)
    {
        var dt = (await _uow.Tokens.FindAsync(
                        t => t.Token == token && t.ExpiresAt > DateTime.UtcNow,
                        null, t => t.Document))
                 .FirstOrDefault();
        if (dt is null) return NotFound("Токен недействителен");

        var doc = dt.Document;
        return File(doc.Data!, "application/octet-stream", doc.Name);
    }
}