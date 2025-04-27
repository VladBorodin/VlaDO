using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.Services;

namespace VlaDO.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class DocumentController : ControllerBase
{
    private readonly DocumentService _docs;
    public DocumentController(DocumentService docs) => _docs = docs;

    [HttpPost("rooms/{roomId:guid}/docs")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Upload(Guid roomId, [FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0) return BadRequest("Файл пуст");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var userId = Guid.Parse(User.FindFirst("nameidentifier")!.Value);
        var id = await _docs.UploadAsync(roomId, userId, file.FileName, ms.ToArray());
        return Ok(new { id });
    }

    [HttpGet("rooms/{roomId:guid}/docs")]
    public async Task<IActionResult> List(Guid roomId)
        => Ok(await _docs.ListAsync(roomId));

    [HttpGet("docs/{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var doc = await _docs.GetAsync(id);
        return doc is null ? NotFound() : File(doc.Data!, "application/octet-stream", doc.Name);
    }

    [HttpDelete("docs/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _docs.DeleteAsync(id);
        return NoContent();
    }
}
