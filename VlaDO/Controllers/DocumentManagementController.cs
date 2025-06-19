using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers
{
    [ApiController, Authorize]
    [Route("api/documents")]
    public class DocumentManagementController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IPermissionService _perm;

        public DocumentManagementController(IUnitOfWork uow, IPermissionService perm)
        {
            _uow = uow;
            _perm = perm;
        }

        [HttpPost("{docId:guid}/archive")]
        public async Task<IActionResult> Archive(Guid docId)
        {
            var userId = User.GetUserId();

            var doc = await _uow.Documents.GetByIdAsync(docId);
            if (doc == null || doc.CreatedBy != userId)
                return Forbid();

            var archiveRoom = await _uow.Rooms
                .FirstOrDefaultAsync(r => r.Title == "Архив" && r.OwnerId == userId);

            if (archiveRoom == null)
            {
                archiveRoom = new Room
                {
                    Id = Guid.NewGuid(),
                    Title = "Архив",
                    OwnerId = userId
                };
                await _uow.Rooms.AddAsync(archiveRoom);
                await _uow.CommitAsync();
            }

            var versions = await _uow.DocumentRepository.GetVersionChainAsync(docId);

            foreach (var version in versions)
            {
                version.RoomId = archiveRoom.Id;

                var tokens = await _uow.Tokens.FindAsync(t =>
                    t.DocumentId == version.Id &&
                    t.UserId != userId);

                foreach (var token in tokens)
                    await _uow.Tokens.DeleteAsync(token.Id);
            }

            await _uow.CommitAsync();
            return Ok();
        }
    }
}
