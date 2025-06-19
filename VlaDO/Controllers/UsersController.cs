using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Repositories;

namespace VlaDO.Controllers;

[ApiController, Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public UsersController(IUnitOfWork uow) => _uow = uow;

    Guid Me => User.GetUserId();

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
    [AllowAnonymous]
    [HttpGet("name-exists")]
    public async Task<IActionResult> NameExists([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            return BadRequest("Некорректное имя");

        var exists = await _uow.Users.AnyAsync(u => u.Name.ToLower() == name.Trim().ToLower());
        return Ok(new { exists });
    }
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
    [HttpGet("getMe")]
    public async Task<IActionResult> GetMe()
    {
        var user = await _uow.Users.GetByIdAsync(User.GetUserId());
        return user is null
            ? NotFound()
            : Ok(new UserBriefDto(user.Id, user.Name));
    }
}
