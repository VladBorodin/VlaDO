using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        private readonly IActivityLogger _logger;

        public DocumentManagementController(IUnitOfWork uow, IPermissionService perm, IActivityLogger logger)
        {
            _uow = uow;
            _perm = perm;
            _logger = logger;
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

            var forkBranch = await _uow.DocumentRepository.GetForkBranchAsync(docId);

            foreach (var version in forkBranch)
            {
                version.RoomId = archiveRoom.Id;

                await _logger.LogAsync(
                    ActivityType.ArchivedDocument,
                    authorId: userId,
                    subjectId: version.Id,
                    meta: new { version.Name, version.Version, version.ForkPath }
                );

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
