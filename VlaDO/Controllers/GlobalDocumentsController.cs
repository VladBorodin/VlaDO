using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Helpers;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers
{
    /// <summary>
    /// Контроллер для работы с глобальными (непривязанными к комнатам) документами.
    /// Позволяет загружать документы, создавать версии и отслеживать историю.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/documents")]
    public class GlobalDocumentsController : ControllerBase
    {
        /// <summary>
        /// Единица работы с репозиториями и базой данных.
        /// </summary>
        private readonly IUnitOfWork _uow;

        /// <summary>
        /// Сервис логирования действий.
        /// </summary>
        private readonly IActivityLogger _logger;

        /// <summary>
        /// Создает экземпляр контроллера глобальных документов.
        /// </summary>
        /// <param name="uow">Интерфейс для доступа к репозиториям.</param>
        /// <param name="logger">Сервис логирования действий пользователя.</param>
        public GlobalDocumentsController(IUnitOfWork uow, IActivityLogger logger)
        {
            _uow = uow;
            _logger = logger;
        }

        /// <summary>
        /// Загружает новый документ, не привязанный к комнате. Поддерживает создание версий на основе предыдущих.
        /// </summary>
        /// <param name="dto">Информация о документе и файле.</param>
        /// <returns>Идентификатор созданного документа.</returns>
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
                RoomId = null
            };

            if (dto.File != null)
            {
                using var ms = new MemoryStream();
                await dto.File.CopyToAsync(ms);
                doc.Data = ms.ToArray();
                doc.Hash = ComputeHash(doc.Data);
            }

            if (!string.IsNullOrWhiteSpace(dto.PrevHash))
            {
                var parent = await _uow.Documents.FirstOrDefaultAsync(d =>
                    d.Hash == dto.PrevHash &&
                    d.RoomId == null &&
                    d.CreatedBy == userId);

                if (parent != null)
                {
                    doc.ParentDocId = parent.Id;
                    doc.PrevHash = parent.Hash;

                    (doc.Version, doc.ForkPath) = await DocumentVersionHelper.GenerateNextVersionAsync(_uow.Documents, parent);
                }
            }

            if (string.IsNullOrEmpty(doc.ForkPath))
            {
                doc.Version = 1;
                doc.ForkPath = await DocumentVersionHelper.SafeGenerateInitialForkPathAsync(_uow.Documents, userId, Guid.Empty);
            }

            await _uow.Documents.AddAsync(doc);
            await _uow.CommitAsync();

            await _logger.LogAsync(ActivityType.CreatedDocument,
                authorId: userId,
                subjectId: doc.Id,
                meta: new { doc.Name });

            return Ok(doc.Id);
        }

        /// <summary>
        /// Вычисляет SHA256-хеш бинарного содержимого файла.
        /// </summary>
        /// <param name="data">Массив байтов файла.</param>
        /// <returns>Строковое представление хеша в нижнем регистре.</returns>
        private static string ComputeHash(byte[] data)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Создает новую версию документа, если он не привязан к комнате.
        /// </summary>
        /// <param name="docId">Идентификатор исходного документа.</param>
        /// <param name="file">Новый файл версии.</param>
        /// <param name="note">Описание или комментарий к новой версии.</param>
        /// <returns>Идентификатор созданной версии документа.</returns>
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

            await _logger.LogAsync(
                ActivityType.UpdatedDocument,
                authorId: userId,
                subjectId: child.Id,
                meta: new { child.Name, child.Version, ForkPath = child.ForkPath }
            );

            return Ok(child.Id);
        }
    }
}