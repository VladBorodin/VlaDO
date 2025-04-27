using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Services;

namespace VlaDO.Controllers;

[ApiController]
[Authorize]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly RoomService _svc;
    public RoomController(RoomService svc) => _svc = svc;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        var ownerId = Guid.Parse(User.FindFirst("nameidentifier")!.Value);
        var id = await _svc.CreateAsync(ownerId, dto.Title);
        return Ok(new { id });
    }

    [HttpGet]
    public async Task<IActionResult> MyRooms()
    {
        var ownerId = Guid.Parse(User.FindFirst("nameidentifier")!.Value);
        return Ok(await _svc.ListAsync(ownerId));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ownerId = Guid.Parse(User.FindFirst("nameidentifier")!.Value);
        await _svc.DeleteAsync(id, ownerId);
        return NoContent();
    }
}
