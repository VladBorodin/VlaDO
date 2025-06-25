using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Org.BouncyCastle.Asn1.Ocsp;
using VlaDO.Services;
using Pipelines.Sockets.Unofficial.Buffers;

namespace VlaDO.Controllers
{
    [ApiController, Authorize]
    [Route("api/contacts")]
    public class ContactsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IActivityLogger _logger;
        public ContactsController(IUnitOfWork uow, IActivityLogger logger) 
        { 
            _uow = uow;
            _logger = logger;
        }

        Guid Me => User.GetUserId();

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var list = await _uow.Contacts
                .FindAsync(c => c.UserId == Me, null, c => c.Contact);

            return Ok(list.Select(c => new UserBriefDto(c.ContactId, c.Contact.Name)));
        }

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
