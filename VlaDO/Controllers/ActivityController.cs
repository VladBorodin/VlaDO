using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers;

[ApiController]
[Authorize]
[Route("api/activity")]
public class ActivityController : ControllerBase
{
    private readonly DocumentFlowContext _ctx;
    private readonly IPermissionService _perm;
    private readonly IActivityReadService _readService;

    public ActivityController(DocumentFlowContext ctx, IPermissionService perm, IActivityReadService readService)
    {
        _ctx = ctx;
        _perm = perm;
        _readService = readService;
    }

    /// <summary>Лента пользователя</summary>
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

    /// <summary>Пометить уведомление прочитанным</summary>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var act = await _ctx.Activities.FindAsync(id);
        if (act is null) return NotFound();

        var userId = User.GetUserId();
        await _readService.MarkAsReadAsync(id, userId);

        return NoContent();
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyPaginatedFeed(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        var uid = User.GetUserId();

        // набор прочитанных
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