using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/documents")]
    public class GlobalDocumentsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public GlobalDocumentsController(IUnitOfWork uow) => _uow = uow;

        [HttpPost]
        public async Task<IActionResult> UploadWithoutRoom([FromForm] CreateDocumentDto dto)
        {
            var userId = User.GetUserId();

            var doc = new Document
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                Note = dto.Note,
                RoomId = null,
                Version = 1
            };

            if (dto.File != null)
            {
                using var ms = new MemoryStream();
                await dto.File.CopyToAsync(ms);
                doc.Data = ms.ToArray();
                doc.Hash = ComputeHash(doc.Data);
            }

            await _uow.Documents.AddAsync(doc);
            await _uow.CommitAsync();

            return Ok(doc.Id);
        }

        private static string ComputeHash(byte[] data)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
        }
    }

}
