using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pipelines.Sockets.Unofficial.Buffers;
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
    private readonly IActivityLogger _logger;

    public RoomController(IRoomService s, IUnitOfWork u, IActivityLogger l) 
    { 
        _svc = s; 
        _uow = u; 
        _logger = l;
    }

    [HttpPost("{roomId:guid}/users")]
    public async Task<IActionResult> AddUser(Guid roomId, [FromBody] AddUserToRoomDto dto)
    {
        var ownerId = User.GetUserId();
        if (!await _uow.Rooms.IsRoomOwnerAsync(roomId, ownerId)) return Forbid();

        var room = await _uow.Rooms.GetByIdAsync(roomId);
        var user = await _uow.Users.GetBriefByIdAsync(ownerId);

        await _svc.AddUserAsync(roomId, dto.UserId, dto.AccessLevel);
        await _uow.CommitAsync();

        await _logger.LogAsync(
            ActivityType.InvitedToRoom,
            authorId: ownerId,
            subjectId: roomId,
            meta: new { RoomTitle = room.Title, UserName = user.Name },
            toUserId: dto.UserId
        );

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

        if (!string.IsNullOrWhiteSpace(dto.Title) &&
            await _uow.Rooms.ExistsWithTitleAsync(userId, dto.Title))
        {
            return Conflict(new { message = "Комната с таким названием уже существует." });
        }

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            OwnerId = userId,
            AccessLevel = (int)(dto.DefaultAccessLevel)
        };
        await _uow.Rooms.AddAsync(room);

        var ownerRecord = new RoomUser
        {
            RoomId = room.Id,
            UserId = userId,
            AccessLevel = AccessLevel.Full
        };
        await _uow.RoomUsers.AddAsync(ownerRecord);

        await _uow.CommitAsync();

        await _logger.LogAsync(
            ActivityType.CreatedRoom,
            authorId: userId,
            subjectId: room.Id,
            meta: new { RoomTitle = room.Title }
        );

        return Ok(room.Id);
    }

    [HttpGet("my")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetMyRooms()
    {
        var userId = User.GetUserId();
        var rooms = await _uow.Rooms.GetByUserAsync(userId);
        var roomIds = rooms.Select(r => r.Id).ToList();

        var accessDict = await _uow.RoomUsers.FindAsync(ru => ru.UserId == userId && roomIds.Contains(ru.RoomId));

        var accessByRoom = accessDict.ToDictionary(ru => ru.RoomId, ru => ru.AccessLevel.ToString());

        var dto = rooms.Select(r => new RoomWithAccessDto(
            r.Id,
            r.Title,
            null,
            accessByRoom.TryGetValue(r.Id, out var level) ? level : "Read"
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
    [HttpGet("grouped")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetGroupedRooms()
    {
        var userId = User.GetUserId();
        var grouped = await _svc.GetGroupedRoomsAsync(userId);
        return Ok(grouped);
    }
    [HttpPatch("{roomId:guid}/rename")]
    public async Task<IActionResult> Rename(Guid roomId, [FromBody] RenameDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Новое имя не может быть пустым");

        var userId = User.GetUserId();
        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room == null) return NotFound();

        if (room.OwnerId != userId) return Forbid();

        room.Title = dto.Name.Trim();
        await _uow.CommitAsync();

        return Ok(room.Id);
    }
    [HttpDelete("{roomId:guid}")]
    public async Task<IActionResult> DeleteRoom(Guid roomId)
    {
        var userId = User.GetUserId();

        var room = await _uow.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null) return NotFound();
        if (room.OwnerId != userId) return Forbid();

        var docs = await _uow.Documents.FindAsync(d => d.RoomId == roomId);
        int count = 0;
        foreach (var d in docs)
        {
            var toks = await _uow.Tokens.FindAsync(t => t.DocumentId == d.Id);
            await _uow.Tokens.DeleteRangeAsync(toks);
            await _uow.Documents.DeleteAsync(d);
            count++;
        }

        var rus = await _uow.RoomUsers.FindAsync(ru => ru.RoomId == roomId);

        await _logger.LogAsync(
            ActivityType.DeletedRoom,
            authorId: userId,
            subjectId: roomId,
            meta: new { RoomTitle = room.Title, Count = count }
        );

        await _uow.RoomUsers.DeleteRangeAsync(rus);

        await _uow.Rooms.DeleteAsync(room);

        await _uow.CommitAsync();
        return NoContent();
    }
    [HttpPatch("{roomId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> UpdateUser(Guid roomId, Guid userId,
                                            [FromBody] UpdateRoomUserDto dto)
    {
        var ownerId = User.GetUserId();
        if (!await _uow.Rooms.IsRoomOwnerAsync(roomId, ownerId)) return Forbid();

        await _uow.Rooms.UpdateUserAccessLevelAsync(roomId, userId, dto.AccessLevel);
        await _uow.CommitAsync();
        return Ok();
    }

    [HttpDelete("{roomId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveUser(Guid roomId, Guid userId)
    {
        var ownerId = User.GetUserId();
        if (!await _uow.Rooms.IsRoomOwnerAsync(roomId, ownerId)) return Forbid();

        await _uow.Rooms.RemoveUserFromRoomAsync(roomId, userId);
        await _uow.CommitAsync();
        return NoContent();
    }

    [HttpDelete("{roomId:guid}/users")]
    public async Task<IActionResult> RemoveAllUsers(Guid roomId)
    {
        var ownerId = User.GetUserId();
        if (!await _uow.Rooms.IsRoomOwnerAsync(roomId, ownerId)) return Forbid();

        var others = await _uow.RoomUsers.FindAsync(ru => ru.RoomId == roomId &&
                                                          ru.UserId != ownerId);
        await _uow.RoomUsers.DeleteRangeAsync(others);
        await _uow.CommitAsync();
        return NoContent();
    }
    [HttpPatch("{roomId:guid}/access-level")]
    public async Task<IActionResult> UpdateAccessLevel(Guid roomId,
    [FromBody] UpdateRoomUserDto dto)
    {
        var ownerId = User.GetUserId();
        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room == null) return NotFound();
        if (room.OwnerId != ownerId) return Forbid();

        room.AccessLevel = (int)dto.AccessLevel;
        await _uow.CommitAsync();

        await _logger.LogAsync(
            ActivityType.UpdatedRoomAccess,
            authorId: ownerId,
            subjectId: roomId,
            meta: new { RoomTitle = room.Title }
        );

        return Ok(room.AccessLevel);
    }
    
    [HttpGet("last-active")]
    public async Task<IActionResult> GetLastActive([FromQuery] int top = 10)
    {
        var uid = User.GetUserId();
        var rooms = await _uow.DocumentRepository.GetLastActiveRoomsAsync(uid, top);
        return Ok(rooms);
    }
}