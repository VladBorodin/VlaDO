using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Helpers;
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
                Version = 1,
                ForkPath = string.Empty
            };

            if (dto.File != null)
            {
                using var ms = new MemoryStream();
                await dto.File.CopyToAsync(ms);
                doc.Data = ms.ToArray();
                doc.Hash = ComputeHash(doc.Data);
                doc.ForkPath = await DocumentVersionHelper.GenerateInitialForkPathAsync(_uow.Documents, userId);
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

        [HttpPost("{docId:guid}/version")]
        public async Task<IActionResult> NewVersion(Guid docId, [FromForm] IFormFile file, [FromForm] string? note)
        {
            var userId = User.GetUserId();

            var parent = await _uow.Documents.GetByIdAsync(docId);
            if (parent is null) return NotFound();
            if (parent.RoomId is not null)
                return StatusCode(405);

            if (parent.CreatedBy != userId) return Forbid();

            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var (nextVersion, nextForkPath) = await DocumentVersionHelper.GenerateNextVersionAsync(_uow.Documents, parent);

            var child = new Document
            {
                Name = file.FileName,
                Data = bytes,
                Note = note,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                Version = nextVersion,
                ForkPath = nextForkPath,
                ParentDocId = parent.Id,
                PrevHash = parent.Hash,
                Hash = ComputeHash(bytes)
            };

            await _uow.Documents.AddAsync(child);
            await _uow.CommitAsync();

            return Ok(child.Id);
        }
    }
}