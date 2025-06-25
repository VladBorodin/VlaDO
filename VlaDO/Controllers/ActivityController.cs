using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers;

/// <summary>
/// Контроллер для работы с пользовательскими активностями (уведомлениями).
/// </summary>
[ApiController]
[Authorize]
[Route("api/activity")]
public class ActivityController : ControllerBase
{
    /// <summary>
    /// Контекст базы данных.
    /// </summary>
    private readonly DocumentFlowContext _ctx;

    /// <summary>
    /// Сервис проверки прав доступа.
    /// </summary>
    private readonly IPermissionService _perm;
    
    /// <summary>
    /// Сервис обработки отметок о прочтении активностей.
    /// </summary>
    private readonly IActivityReadService _readService;

    /// <summary>
    /// Инициализирует контроллер активностей.
    /// </summary>
    /// <param name="ctx">Контекст базы данных.</param>
    /// <param name="perm">Сервис прав доступа.</param>
    /// <param name="readService">Сервис отметок о прочтении.</param>
    public ActivityController(DocumentFlowContext ctx, IPermissionService perm, IActivityReadService readService)
    {
        _ctx = ctx;
        _perm = perm;
        _readService = readService;
    }

    /// <summary>
    /// Получает список непрочитанных активностей пользователя.
    /// </summary>
    /// <param name="top">Максимальное количество записей (по умолчанию 20).</param>
    /// <returns>Список непрочитанных активностей.</returns>
    [HttpGet]
    public async Task<IActionResult> GetMyFeed([FromQuery] int top = 20)
    {
        var uid = User.GetUserId();

        var readIds = await _ctx.ActivityReads
            .Where(ar => ar.UserId == uid)
            .Select(ar => ar.ActivityId)
            .ToListAsync();

        var q = _ctx.Activities.Where(a => a.UserId == uid);

        var roomIds = await _perm.GetRoomsWithAccessAsync(uid, AccessLevel.Read);
        var docIds = await _perm.GetDocsWithAccessAsync(uid, AccessLevel.Read);

        q = q.Union(_ctx.Activities.Where(a =>
                     a.UserId == null &&
                     ((a.RoomId != null && roomIds.Contains(a.RoomId.Value)) ||
                      (a.DocumentId != null && docIds.Contains(a.DocumentId.Value)))));

        q = q.Where(a => !readIds.Contains(a.Id));

        var items = await q.OrderByDescending(a => a.CreatedAt)
                           .Take(top)
                           .Select(a => new ActivityDto(a))
                           .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Получает последние активности пользователя для дашборда (включая прочитанные).
    /// </summary>
    /// <param name="top">Максимальное количество записей (по умолчанию 10).</param>
    /// <returns>Список последних активностей.</returns>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardFeed([FromQuery] int top = 10)
    {
        var uid = User.GetUserId();

        var roomIds = await _perm.GetRoomsWithAccessAsync(uid, AccessLevel.Read);
        var docIds = await _perm.GetDocsWithAccessAsync(uid, AccessLevel.Read);

        var q = _ctx.Activities.Where(a =>
            (a.UserId == uid) ||
            (a.UserId == null &&
             ((a.RoomId != null && roomIds.Contains(a.RoomId.Value)) ||
              (a.DocumentId != null && docIds.Contains(a.DocumentId.Value)))
            ));

        var items = await q.OrderByDescending(a => a.CreatedAt)
                           .Take(top)
                           .Select(a => new ActivityDto(a))
                           .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Помечает указанную активность как прочитанную.
    /// </summary>
    /// <param name="id">Идентификатор активности.</param>
    /// <returns>Код 204 при успешной отметке.</returns>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var act = await _ctx.Activities.FindAsync(id);
        if (act is null) return NotFound();

        var userId = User.GetUserId();
        await _readService.MarkAsReadAsync(id, userId);

        return NoContent();
    }

    /// <summary>
    /// Получает постраничный список активностей пользователя.
    /// </summary>
    /// <param name="page">Номер страницы (по умолчанию 1).</param>
    /// <param name="pageSize">Размер страницы (по умолчанию 10).</param>
    /// <returns>Постраничный список активностей с информацией о прочтении.</returns>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyPaginatedFeed(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        var uid = User.GetUserId();

        var readIds = await _ctx.ActivityReads
            .Where(ar => ar.UserId == uid)
            .Select(ar => ar.ActivityId)
            .ToListAsync();

        var roomIds = await _perm.GetRoomsWithAccessAsync(uid, AccessLevel.Read);
        var docIds = await _perm.GetDocsWithAccessAsync(uid, AccessLevel.Read);

        var q = _ctx.Activities.Where(a =>
            a.UserId == uid ||
            (a.UserId == null &&
             ((a.RoomId != null && roomIds.Contains(a.RoomId.Value)) ||
              (a.DocumentId != null && docIds.Contains(a.DocumentId.Value)))));

        var total = await q.CountAsync();

        var items = await q.OrderByDescending(a => a.CreatedAt)
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .Select(a => new ActivityDtoExt(new ActivityDto(a),readIds.Contains(a.Id)))
                           .ToListAsync();

        return Ok(new PagedResult<ActivityDtoExt>(items,(int)Math.Ceiling(total / (double)pageSize)));
    }
}