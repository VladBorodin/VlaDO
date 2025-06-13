using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.DTOs.Room;
using VlaDO.Extensions;
using VlaDO.Models;
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
    [HttpGet("recent")]
    public async Task<IActionResult> Recent([FromQuery] int take = 3)
    {
        var list = await _uow.Rooms.GetRecentAsync(User.GetUserId(), take);
        return Ok(list);
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        var userId = User.GetUserId();

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            OwnerId = userId
        };

        await _uow.Rooms.AddAsync(room);

        var ownerRecord = new RoomUser
        {
            RoomId = room.Id,
            UserId = userId,
            AccessLevel = AccessLevel.Admin
        };
        await _uow.RoomUsers.AddAsync(ownerRecord);

        await _uow.CommitAsync();
        return Ok(room.Id);
    }
    [HttpGet("my")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetMyRooms()
    {
        var userId = User.GetUserId();
        var rooms = await _uow.Rooms.GetByUserAsync(userId);

        var dto = rooms.Select(r => new RoomBriefDto(
            r.Id,
            r.Title,
            null
        ));

        return Ok(dto);
    }
    [HttpPost("search")]
    public async Task<IActionResult> SearchRooms([FromBody] RoomFilterDto filter)
    {
        var userId = User.GetUserId();
        var rooms = await _uow.Rooms.SearchRoomsAsync(userId, filter.Title, filter.RoomId, filter.Since);
        return Ok(rooms);
    }

}