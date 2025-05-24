using Microsoft.AspNetCore.Mvc;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private IContactRepository _contactRepository;
        public ContactController(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var contacts = await _contactRepository.GetAllContacts();
            if (contacts == null)
            {
                return NotFound();
            }
            return Ok(contacts);
        }
    }
}
