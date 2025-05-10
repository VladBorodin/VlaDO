using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers;

[ApiController, Authorize, Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _svc;
    private readonly IUnitOfWork _uow;
    public RoomController(IRoomService s, IUnitOfWork u) { _svc = s; _uow = u; }

    [HttpPost("{roomId:guid}/users")]
    public async Task<IActionResult> AddUser(Guid roomId, [FromBody] AddUserToRoomDto dto)
    {
        var ownerId = User.GetUserId();
        if (!await _uow.Rooms.IsRoomOwnerAsync(roomId, ownerId)) return Forbid();

        await _svc.AddUserAsync(roomId, dto.UserId, dto.AccessLevel);
        await _uow.CommitAsync();
        return Ok();
    }

    [HttpGet("{roomId:guid}/users")]
    public async Task<IActionResult> GetUsers(Guid roomId)
    {
        var rus = await _uow.RoomUsers
            .FindAsync(ru => ru.RoomId == roomId, null, ru => ru.User);
        var result = rus.Select(ru => new RoomUserDto(ru.UserId, ru.User.Name, ru.AccessLevel));
        return Ok(result);
    }
}