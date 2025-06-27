using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers
{

    /// <summary>
    /// Контроллер для управления действиями архивации документов.
    /// </summary>
    [ApiController, Authorize]
    [Route("api/documents")]
    public class DocumentManagementController : ControllerBase
    {
        /// <summary>
        /// Единица работы с репозиториями.
        /// </summary>
        private readonly IUnitOfWork _uow;

        /// <summary>
        /// Сервис проверки прав доступа.
        /// </summary>
        private readonly IPermissionService _perm;

        /// <summary>
        /// Сервис логирования пользовательских действий.
        /// </summary>
        private readonly IActivityLogger _logger;

        /// <summary>
        /// Создает экземпляр контроллера для управления архивацией документов.
        /// </summary>
        /// <param name="uow">Единица работы с репозиториями.</param>
        /// <param name="perm">Сервис прав доступа.</param>
        /// <param name="logger">Сервис логирования.</param>
        public DocumentManagementController(IUnitOfWork uow, IPermissionService perm, IActivityLogger logger)
        {
            _uow = uow;
            _perm = perm;
            _logger = logger;
        }

        /// <summary>
        /// Архивирует документ и все его версии, перемещая их в комнату "Архив".
        /// Удаляет доступ по токенам других пользователей.
        /// </summary>
        /// <param name="docId">Идентификатор документа для архивации.</param>
        /// <returns>Результат архивации: Ok или ошибка доступа.</returns>
        [HttpPost("{docId:guid}/archive")]
        public async Task<IActionResult> Archive(Guid docId)
        {
            var userId = User.GetUserId();

            var doc = await _uow.Documents.GetByIdAsync(docId);
            if (doc == null || doc.CreatedBy != userId)
                return Forbid();

            var archiveRoom = await _uow.Rooms
                .FirstOrDefaultAsync(r => r.OwnerId == userId && r.Title == "Архив");

            var forkBranch = await _uow.DocumentRepository.GetForkBranchAsync(docId, userId);

            foreach (var version in forkBranch)
            {
                version.RoomId = archiveRoom.Id;

                await _logger.LogAsync(
                    ActivityType.ArchivedDocument,
                    authorId: userId,
                    subjectId: version.Id,
                    meta: new { version.Name, version.Version, version.ForkPath });

                var tokens = await _uow.Tokens.FindAsync(t =>
                    t.DocumentId == version.Id && t.UserId != userId);

                foreach (var token in tokens)
                    await _uow.Tokens.DeleteAsync(token.Id);
            }

            await _uow.CommitAsync();
            return Ok();
        }
    }
}
