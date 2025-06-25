using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Repositories;

namespace VlaDO.Controllers;

/// <summary>
/// Контроллер для работы с пользователями.
/// </summary>
[ApiController, Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Репозиторий для работы с данными.
    /// </summary>
    private readonly IUnitOfWork _uow;

    /// <summary>
    /// Инициализирует новый экземпляр контроллера пользователей.
    /// </summary>
    /// <param name="uow">Объект UnitOfWork для доступа к данным.</param>
    public UsersController(IUnitOfWork uow) => _uow = uow;

    /// <summary>
    /// Получить идентификатор текущего пользователя.
    /// </summary>
    Guid Me => User.GetUserId();

    /// <summary>
    /// Поиск пользователей по имени или email, исключая уже добавленных в контакты и самого себя.
    /// </summary>
    /// <param name="q">Термин поиска (альтернатива <paramref name="query"/>).</param>
    /// <param name="query">Термин поиска (альтернатива <paramref name="q"/>).</param>
    /// <returns>Список найденных пользователей в виде краткой информации.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string? q, [FromQuery(Name = "query")] string? query)
    {
        var term = (q ?? query)?.Trim();

        if (string.IsNullOrWhiteSpace(term))
            return Ok(Array.Empty<UserBriefDto>());

        var skipIds = (await _uow.Contacts
                        .FindAsync(c => c.UserId == Me))
                       .Select(c => c.ContactId)
                       .Append(Me)
                       .ToHashSet();

        term = term.ToLowerInvariant();

        var users = await _uow.Users.FindAsync(
            u => !skipIds.Contains(u.Id) &&
                (u.Name.ToLower().Contains(term) ||
                 u.Email.ToLower().Contains(term)),
            orderBy: q => q
                .OrderBy(u => !u.Name.ToLower().StartsWith(term))
                .ThenBy(u => !u.Email.ToLower().StartsWith(term))
                .ThenBy(u => u.Name));

        return Ok(users.Take(20)
                       .Select(u => new UserBriefDto(u.Id, u.Name)));
    }

    /// <summary>
    /// Проверка, существует ли пользователь с указанным именем.
    /// </summary>
    /// <param name="name">Имя пользователя для проверки.</param>
    /// <returns>Объект с флагом <c>exists</c>, указывающим на существование имени.</returns>
    [AllowAnonymous]
    [HttpGet("name-exists")]
    public async Task<IActionResult> NameExists([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            return BadRequest("Некорректное имя");

        var exists = await _uow.Users.AnyAsync(u => u.Name.ToLower() == name.Trim().ToLower());
        return Ok(new { exists });
    }

    /// <summary>
    /// Обновить имя и email текущего пользователя.
    /// </summary>
    /// <param name="dto">Объект с новым именем и email.</param>
    /// <returns>Обновлённая краткая информация о пользователе.</returns>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto dto)
    {
        var me = User.GetUserId();
        var user = await _uow.Users.GetByIdAsync(me);
        if (user == null) return NotFound();

        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var busy = await _uow.Users.AnyAsync(u =>
            u.Email.ToLower() == dto.Email.Trim().ToLower() &&
            u.Id != me);
            if (busy) return Conflict("Email уже зарегистрирован");
        }
        
        if (!string.Equals(user.Name, dto.Name, StringComparison.Ordinal))
        {
            var taken = await _uow.Users.AnyAsync(u =>
            u.Name.ToLower() == dto.Name.Trim().ToLower() &&
            u.Id != me);
            if (taken) return Conflict("Имя уже занято");
        }

        user.Name = dto.Name;
        user.Email = dto.Email;
        await _uow.CommitAsync();

        return Ok(new UserBriefDto(user.Id, user.Name));
    }

    /// <summary>
    /// Получить информацию о текущем пользователе.
    /// </summary>
    /// <returns>Краткая информация о пользователе.</returns>
    [HttpGet("getMe")]
    public async Task<IActionResult> GetMe()
    {
        var user = await _uow.Users.GetByIdAsync(User.GetUserId());
        return user is null
            ? NotFound()
            : Ok(new UserBriefDto(user.Id, user.Name));
    }
}
