using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Extensions;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers
{
    /// <summary>
    /// Контроллер для управления контактами пользователя.
    /// </summary>
    [ApiController, Authorize]
    [Route("api/contacts")]
    public class ContactsController : ControllerBase
    {
        /// <summary>
        /// Интерфейс для работы с репозиториями.
        /// </summary>
        private readonly IUnitOfWork _uow;

        /// <summary>
        /// Сервис для логирования действий.
        /// </summary>
        private readonly IActivityLogger _logger;

        /// <summary>
        /// Конструктор контроллера контактов.
        /// </summary>
        /// <param name="uow">Единица работы с базой данных.</param>
        /// <param name="logger">Сервис логирования активности.</param>
        public ContactsController(IUnitOfWork uow, IActivityLogger logger) 
        { 
            _uow = uow;
            _logger = logger;
        }

        /// <summary>
        /// Получает ID текущего пользователя.
        /// </summary>
        Guid Me => User.GetUserId();

        /// <summary>
        /// Получает список контактов текущего пользователя.
        /// </summary>
        /// <returns>Список кратких описаний пользователей.</returns>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var list = await _uow.Contacts
                .FindAsync(c => c.UserId == Me, null, c => c.Contact);

            return Ok(list.Select(c => new UserBriefDto(c.ContactId, c.Contact.Name)));
        }

        /// <summary>
        /// Добавляет пользователя в список контактов.
        /// </summary>
        /// <param name="contactId">Идентификатор пользователя, которого нужно добавить.</param>
        /// <returns>Код 200 при успехе или 400 при ошибке.</returns>
        [HttpPost("{contactId:guid}")]
        public async Task<IActionResult> Add(Guid contactId)
        {
            var user = await _uow.Users.GetBriefByIdAsync(Me);

            if (contactId == Me ||
                await _uow.Contacts.AnyAsync(c => c.UserId == Me && c.ContactId == contactId))
                return BadRequest();

            var newContact = new UserContact { Id = Guid.NewGuid(), UserId = Me, ContactId = contactId };

            await _uow.Contacts.AddAsync(newContact);
            await _uow.CommitAsync();

            await _logger.LogAsync(
                ActivityType.InvitedToContacts,
                authorId: Me,
                subjectId: newContact.Id,
                toUserId: contactId,
                meta: new { 
                    UserName = user.Name,
                    ContactId = Me
                }
            );

            return Ok();
        }

        /// <summary>
        /// Удаляет пользователя из списка контактов.
        /// </summary>
        /// <param name="contactId">Идентификатор удаляемого контакта.</param>
        /// <returns>Код 204 при успехе или 404 если не найден.</returns>
        [HttpDelete("{contactId:guid}")]
        public async Task<IActionResult> Remove(Guid contactId)
        {
            var contact = await _uow.Contacts
                .FirstOrDefaultAsync(c => c.UserId == Me && c.ContactId == contactId);

            if (contact == null) return NotFound();

            await _uow.Contacts.DeleteAsync(contact.ContactId);
            await _uow.CommitAsync();
            return NoContent();
        }

        /// <summary>
        /// Принимает входящий запрос на добавление в контакты.
        /// </summary>
        /// <param name="contactId">Идентификатор пользователя, отправившего запрос.</param>
        /// <returns>Код 204 при успехе или 404 если не найден запрос.</returns>
        [HttpPost("{contactId:guid}/accept")]
        public async Task<IActionResult> Accept(Guid contactId)
        {
            var me = Me;
            var requester = contactId;

            var request = await _uow.Contacts.FirstOrDefaultAsync(
                c => c.UserId == requester && c.ContactId == me);
            if (request is null) return NotFound();

            if (!await _uow.Contacts.AnyAsync(c => c.UserId == me && c.ContactId == requester))
                await _uow.Contacts.AddAsync(new UserContact
                {
                    Id = Guid.NewGuid(),
                    UserId = me,
                    ContactId = requester
                });

            var meName = (await _uow.Users.GetBriefByIdAsync(me))?.Name ?? "Пользователь";

            await _logger.LogAsync(
                ActivityType.AcceptedContact,
                authorId: me,
                subjectId: request.Id,
                toUserId: requester,
                meta: new { UserName = meName }
            );

            await _uow.CommitAsync();
            return NoContent();
        }

        /// <summary>
        /// Отклоняет или блокирует входящий запрос на добавление в контакты.
        /// </summary>
        /// <param name="contactId">Идентификатор пользователя, чей запрос отклоняется.</param>
        /// <returns>Код 204 при успехе или 404 если не найден запрос.</returns>
        [HttpPost("{contactId:guid}/block")]
        public async Task<IActionResult> Block(Guid contactId)
        {
            var me = Me;
            var requester = contactId;

            var request = await _uow.Contacts.FirstOrDefaultAsync(
                c => c.UserId == requester && c.ContactId == me);
            if (request is null) return NotFound();

            var meName = (await _uow.Users.GetBriefByIdAsync(me))?.Name ?? "Пользователь";

            await _logger.LogAsync(
                ActivityType.DeclinedContact,
                authorId: me,
                subjectId: request.Id,
                toUserId: requester,
                meta: new { UserName = meName }
            );

            await _uow.Contacts.DeleteAsync(request);

            await _uow.CommitAsync();
            return NoContent();
        }
    }
}
