using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using VlaDO.DTOs;
using VlaDO.DTOs.Room;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers;

/// <summary>
/// Контроллер для управления комнатами (создание, приглашение пользователей, получение доступа).
/// </summary>
[ApiController, Authorize, Route("api/rooms")]
public class RoomController : ControllerBase
{
    /// <summary>
    /// Сервис управления комнатами.
    /// </summary>
    private readonly IRoomService _svc;

    /// <summary>
    /// Интерфейс доступа к репозиториям.
    /// </summary>
    private readonly IUnitOfWork _uow;

    /// <summary>
    /// Сервис логирования действий.
    /// </summary>
    private readonly IActivityLogger _logger;

    /// <summary>
    /// Конструктор контроллера комнат.
    /// </summary>
    /// <param name="s">Сервис комнат.</param>
    /// <param name="u">Единица работы с базой данных.</param>
    /// <param name="l">Логгер активностей.</param>
    public RoomController(IRoomService s, IUnitOfWork u, IActivityLogger l) 
    { 
        _svc = s; 
        _uow = u; 
        _logger = l;
    }

    /// <summary>
    /// Пригласить пользователя в комнату.
    /// </summary>
    /// <param name="roomId">ID комнаты.</param>
    /// <param name="dto">Информация о пользователе и уровне доступа.</param>
    /// <returns>Результат операции.</returns>
    [HttpPost("{roomId:guid}/users")]
    public async Task<IActionResult> AddUser(Guid roomId, [FromBody] AddUserToRoomDto dto)
    {
        var ownerId = User.GetUserId();
        if (!await _uow.Rooms.IsRoomOwnerAsync(roomId, ownerId)) return Forbid();

        var room = await _uow.Rooms.GetByIdAsync(roomId);
        var user = await _uow.Users.GetBriefByIdAsync(ownerId);

        await _logger.LogAsync(
            ActivityType.InvitedToRoom,
            authorId: ownerId,
            subjectId: roomId,
            toUserId: dto.UserId,
            meta: new { RoomTitle = room.Title, UserName = user.Name }
        );

        await _uow.CommitAsync();

        return Ok();
    }


    /// <summary>
    /// Получить список пользователей комнаты.
    /// </summary>
    /// <param name="roomId">ID комнаты.</param>
    /// <returns>Список пользователей с их уровнями доступа.</returns>
    [HttpGet("{roomId:guid}/users")]
    public async Task<IActionResult> GetUsers(Guid roomId)
    {
        var rus = await _uow.RoomUsers
            .FindAsync(ru => ru.RoomId == roomId, null, ru => ru.User);
        var result = rus.Select(ru => new RoomUserDto(ru.UserId, ru.User.Name, ru.AccessLevel));
        return Ok(result);
    }

    /// <summary>
    /// Получить список последних комнат, с которыми взаимодействовал пользователь.
    /// </summary>
    /// <param name="take">Сколько комнат вернуть (по умолчанию — 3).</param>
    /// <returns>Список комнат.</returns>
    [HttpGet("recent")]
    public async Task<IActionResult> Recent([FromQuery] int take = 3)
    {
        var list = await _uow.Rooms.GetRecentAsync(User.GetUserId(), take);
        return Ok(list);
    }

    /// <summary>
    /// Создать новую комнату.
    /// </summary>
    /// <param name="dto">Данные о создаваемой комнате.</param>
    /// <returns>ID новой комнаты или ошибка.</returns>
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

    /// <summary>
    /// Получить список комнат, доступных текущему пользователю, с указанием уровня доступа.
    /// </summary>
    /// <returns>Список комнат с доступом.</returns>
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

    /// <summary>
    /// Поиск комнат по фильтру: названию, идентификатору или дате.
    /// </summary>
    /// <param name="filter">Фильтр поиска комнат.</param>
    /// <returns>Список подходящих комнат.</returns>
    [HttpPost("search")]
    public async Task<IActionResult> SearchRooms([FromBody] RoomFilterDto filter)
    {
        var userId = User.GetUserId();
        var rooms = await _uow.Rooms.SearchRoomsAsync(userId, filter.Title, filter.RoomId, filter.Since);
        return Ok(rooms);
    }

    /// <summary>
    /// Получить список комнат, сгруппированных по доступу (мои и чужие).
    /// </summary>
    /// <returns>Сгруппированный список комнат.</returns>
    [HttpGet("grouped")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetGroupedRooms()
    {
        var userId = User.GetUserId();
        var grouped = await _svc.GetGroupedRoomsAsync(userId);
        return Ok(grouped);
    }

    /// <summary>
    /// Переименовать комнату.
    /// </summary>
    /// <param name="roomId">ID комнаты.</param>
    /// <param name="dto">Новые данные, включая имя.</param>
    /// <returns>ID обновлённой комнаты или ошибка.</returns>
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

    /// <summary>
    /// Удалить комнату и все связанные с ней документы и доступы.
    /// </summary>
    /// <param name="roomId">ID удаляемой комнаты.</param>
    /// <returns>HTTP 204 при успехе.</returns>
    [HttpDelete("{roomId:guid}")]
    public async Task<IActionResult> DeleteRoom(Guid roomId)
    {
        var userId = User.GetUserId();

        var room = await _uow.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null) return NotFound();
        if (room.OwnerId != userId) return Forbid();

        var participants = (await _uow.RoomUsers
            .FindAsync(ru => ru.RoomId == roomId))
            .Select(ru => ru.UserId)
            .Append(userId)
            .Distinct()
            .ToList();

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

        await _logger.LogForUsersAsync(
            ActivityType.DeletedRoom,
            participants,
            authorId: userId,
            roomId: roomId,
            meta: new { RoomTitle = room.Title, Count = count }
        );

        await _uow.RoomUsers.DeleteRangeAsync(rus);
        await _uow.Rooms.DeleteAsync(room);
        await _uow.CommitAsync();

        return NoContent();
    }

    /// <summary>
    /// Обновить уровень доступа пользователя в комнате.
    /// </summary>
    /// <param name="roomId">ID комнаты.</param>
    /// <param name="userId">ID пользователя, которому обновляется доступ.</param>
    /// <param name="dto">Новый уровень доступа.</param>
    /// <returns>HTTP 200 при успехе.</returns>
    [HttpPatch("{roomId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> UpdateUser(Guid roomId, Guid userId,[FromBody] UpdateRoomUserDto dto)
    {
        var ownerId = User.GetUserId();
        if (!await _uow.Rooms.IsRoomOwnerAsync(roomId, ownerId)) return Forbid();

        await _uow.Rooms.UpdateUserAccessLevelAsync(roomId, userId, dto.AccessLevel);
        await _uow.CommitAsync();
        return Ok();
    }

    /// <summary>
    /// Удалить пользователя из комнаты.
    /// </summary>
    /// <param name="roomId">ID комнаты.</param>
    /// <param name="userId">ID пользователя, которого необходимо удалить.</param>
    /// <returns>HTTP 204 при успехе.</returns>
    [HttpDelete("{roomId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveUser(Guid roomId, Guid userId)
    {
        var ownerId = User.GetUserId();
        var room = await _uow.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
        if (!await _uow.Rooms.IsRoomOwnerAsync(roomId, ownerId)) return Forbid();

        await _logger.LogAsync(
            ActivityType.RevokedRoom,
            authorId: ownerId,
            subjectId: roomId,
            toUserId: userId,
            meta: new { RoomTitle = room.Title}
        );

        await _uow.Rooms.RemoveUserFromRoomAsync(roomId, userId);
        await _uow.CommitAsync();
        return NoContent();
    }

    /// <summary>
    /// Удалить всех пользователей, кроме владельца, из комнаты.
    /// </summary>
    /// <param name="roomId">ID комнаты.</param>
    /// <returns>HTTP 204 при успехе.</returns>
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

    /// <summary>
    /// Обновить уровень доступа по умолчанию для новых пользователей комнаты.
    /// </summary>
    /// <param name="roomId">ID комнаты.</param>
    /// <param name="dto">Новый уровень доступа.</param>
    /// <returns>Обновлённый уровень доступа.</returns>
    [HttpPatch("{roomId:guid}/access-level")]
    public async Task<IActionResult> UpdateAccessLevel(Guid roomId, [FromBody] UpdateRoomUserDto dto)
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

    /// <summary>
    /// Получить список комнат, в которых пользователь был активен последним.
    /// </summary>
    /// <param name="top">Максимальное количество возвращаемых комнат (по умолчанию 10).</param>
    /// <returns>Список последних активных комнат.</returns>
    [HttpGet("last-active")]
    public async Task<IActionResult> GetLastActive([FromQuery] int top = 10)
    {
        var uid = User.GetUserId();
        var rooms = await _uow.DocumentRepository.GetLastActiveRoomsAsync(uid, top);
        return Ok(rooms);
    }

    /// <summary>
    /// Принять приглашение в комнату.
    /// </summary>
    /// <param name="roomId">ID комнаты, в которую пользователь был приглашён.</param>
    /// <returns>HTTP 204 при успехе, 403 если не было приглашения, 404 если комната не найдена.</returns>
    [HttpPost("{roomId:guid}/accept")]
    public async Task<IActionResult> AcceptInvite(Guid roomId)
    {
        var uid = User.GetUserId();
        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room is null) return NotFound();

        var hadInvite = await _uow.Activities.AnyAsync(a =>
            a.Type == ActivityType.InvitedToRoom &&
            a.RoomId == roomId &&
            a.UserId == uid);

        if (!hadInvite) return Forbid();

        await _uow.RoomUsers.AddAsync(new RoomUser
        {
            RoomId = roomId,
            UserId = uid,
            AccessLevel = AccessLevel.Read
        });
        await _uow.CommitAsync();

        await _logger.LogAsync(
            ActivityType.AcceptedRoom,
            authorId: uid,
            subjectId: roomId,
            toUserId: room.OwnerId,
            meta: new { UserName = (await _uow.Users.GetBriefByIdAsync(uid)).Name });

        return NoContent();
    }

    /// <summary>
    /// Отклонить приглашение в комнату.
    /// </summary>
    /// <param name="roomId">ID комнаты, приглашение в которую отклоняется.</param>
    /// <returns>HTTP 204 при успехе, 403 если не было приглашения, 404 если комната не найдена.</returns>
    [HttpPost("{roomId:guid}/decline")]
    public async Task<IActionResult> DeclineInvite(Guid roomId)
    {
        var uid = User.GetUserId();
        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room is null) return NotFound();

        var hadInvite = await _uow.Activities.AnyAsync(a =>
            a.Type == ActivityType.InvitedToRoom &&
            a.RoomId == roomId &&
            a.UserId == uid);

        if (!hadInvite) return Forbid();

        await _logger.LogAsync(
            ActivityType.DeclinedRoom,
            authorId: uid,
            subjectId: roomId,
            toUserId: room.OwnerId,
            meta: new { UserName = (await _uow.Users.GetBriefByIdAsync(uid)).Name });

        return NoContent();
    }

}