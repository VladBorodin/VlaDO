using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Extensions;

namespace VlaDO.Controllers
{
    [ApiController, Authorize]
    [Route("api/contacts")]
    public class ContactsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public ContactsController(IUnitOfWork uow) => _uow = uow;

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
            if (contactId == Me ||
                await _uow.Contacts.AnyAsync(c => c.UserId == Me && c.ContactId == contactId))
                return BadRequest();

            await _uow.Contacts.AddAsync(new UserContact { UserId = Me, ContactId = contactId });
            await _uow.CommitAsync();
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
    }

}
