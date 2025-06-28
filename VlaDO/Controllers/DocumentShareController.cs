using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers;

/// <summary>
/// Контроллер управления доступом к документам через токены.
/// </summary>
[ApiController, Authorize]
[Route("api/documents/{docId:guid}")]
public class DocumentTokenController : ControllerBase
{
    /// <summary>
    /// Единица работы с репозиториями.
    /// </summary>
    private readonly IUnitOfWork _uow;

    /// <summary>
    /// Сервис проверки прав доступа.
    /// </summary>
    private readonly IPermissionService _perm;

    /// <summary>
    /// Сервис логирования действий пользователей.
    /// </summary>
    private readonly IActivityLogger _logger;

    /// <summary>
    /// Репозиторий документов.
    /// </summary>
    private IGenericRepository<Document> Docs => _uow.Documents;

    /// <summary>
    /// Конструктор контроллера управления токенами доступа.
    /// </summary>
    /// <param name="uow">Единица работы с БД.</param>
    /// <param name="perm">Сервис прав доступа.</param>
    /// <param name="logger">Сервис логирования.</param>
    public DocumentTokenController(IUnitOfWork uow, IPermissionService perm, IActivityLogger logger)
    {
        _uow = uow;
        _perm = perm;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех токенов доступа к документу.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>Список токенов с указанием прав и пользователей.</returns>
    [HttpGet("tokens")]
    public async Task<IActionResult> List(Guid docId)
    {
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

    /// <summary>
    /// Добавляет или обновляет токен доступа к документу для указанного пользователя.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <param name="dto">Данные для обновления доступа.</param>
    /// <returns>Результат выполнения запроса.</returns>
    /// <exception cref="KeyNotFoundException">Документ не найден.</exception>
    [HttpPost("token")]
    public async Task<IActionResult> Upsert(Guid docId, [FromBody] UpdateAccessDto dto)
    {
        var authorId = User.GetUserId();
        var doc = await _uow.Documents.GetByIdAsync(docId)
              ?? throw new KeyNotFoundException();
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

            await _logger.LogAsync(
                ActivityType.IssuedToken,
                authorId: authorId,
                subjectId: tok.Id,
                meta: new { doc.Name },
                toUserId: dto.UserId
            );
        }
        else
        {
            tok.AccessLevel = dto.AccessLevel;
        }

        await _logger.LogAsync(
            ActivityType.UpdatedToken,
            authorId: authorId,
            subjectId: tok.Id,
            meta: new { doc.Name },
            toUserId: dto.UserId
        );

        await _uow.CommitAsync();
        return Ok();
    }

    /// <summary>
    /// Удаляет токен доступа к документу по его идентификатору.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <param name="tokenId">Идентификатор токена.</param>
    /// <returns>Результат выполнения: NoContent или ошибка доступа.</returns>
    [HttpDelete("token/{tokenId:guid}")]
    public async Task<IActionResult> Delete(Guid docId, Guid tokenId)
    {
        var authorId = User.GetUserId();

        var doc = await _uow.Documents.GetByIdAsync(docId);
        if (doc == null) return NotFound();

        if (!await _perm.CheckAccessAsync(authorId, docId, AccessLevel.Full))
            return Forbid();

        var tok = await _uow.Tokens.GetByIdAsync(tokenId);
        if (tok == null || tok.DocumentId != docId)
            return NotFound();

        if (!await _perm.CheckAccessAsync(User.GetUserId(), docId, AccessLevel.Full))
            return Forbid();

        await _logger.LogAsync(
            ActivityType.RevokedToken,
            authorId: authorId,
            subjectId: tokenId,
            meta: new { doc.Name },
            toUserId: tok.UserId
        );

        await _uow.Tokens.DeleteAsync(tokenId);
        await _uow.CommitAsync();
        return NoContent();
    }

    /// <summary>
    /// Получает список пользователей, с которыми документ был разделён.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>Список пользователей с доступом.</returns>
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

    /// <summary>
    /// Удаляет токен доступа к документу для конкретного пользователя.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <param name="userId">Идентификатор пользователя, чей доступ необходимо отозвать.</param>
    /// <returns>Результат выполнения: NoContent или NotFound.</returns>
    [HttpDelete("token/user/{userId:guid}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> DeleteByUser(Guid docId, Guid userId)
    {
        var authorId = User.GetUserId();

        var doc = await _uow.Documents.GetByIdAsync(docId);
        if (doc == null) return NotFound();

        var token = await _uow.Tokens
            .FirstOrDefaultAsync(t => t.DocumentId == docId && t.UserId == userId);

        if (token == null)
            return NotFound();

        await _logger.LogAsync(
            ActivityType.RevokedToken,
            authorId: authorId,
            subjectId: token.Id,
            meta: new { doc.Name },
            toUserId: token.UserId
        );

        await _uow.Tokens.DeleteAsync(token.Id);
        await _uow.CommitAsync();

        return NoContent();
    }
}