using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

using System.Linq;

using XLead_Server.DTOs;

using XLead_Server.Interfaces;

using Microsoft.Extensions.Logging;

using System.Collections.Generic;

namespace XLead_Server.Controllers

{

    [ApiController]

    [Route("api/[controller]")]

    public class NotesController : ControllerBase

    {

        private readonly INoteRepository _noteRepository;

        private readonly IUserPrivilegeRepository _userPrivilegeRepository;

        private readonly ILogger<NotesController> _logger;

        public NotesController(

            INoteRepository noteRepository,

            IUserPrivilegeRepository userPrivilegeRepository,

            ILogger<NotesController> logger)

        {

            _noteRepository = noteRepository;

            _userPrivilegeRepository = userPrivilegeRepository;

            _logger = logger;

        }

        [HttpPost]

        [ProducesResponseType(typeof(NoteReadDto), 201)]

        [ProducesResponseType(400)]

        [ProducesResponseType(403)]

        public async Task<ActionResult<NoteReadDto>> CreateNote([FromBody] NoteCreateDto dto)

        {

            _logger.LogInformation($"Creating note for deal {dto.DealId} by user {dto.CreatedBy}");

            // Check privileges

            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync((int)dto.CreatedBy);

            if (!privileges.Any(p => p.PrivilegeName == "CreateNote" || p.PrivilegeName == "PipelineDetailAccess"))

            {

                _logger.LogWarning($"User {dto.CreatedBy} lacks CreateNote privilege");

                return Forbid("User lacks CreateNote privilege");

            }

            if (!ModelState.IsValid)

            {

                return BadRequest(ModelState);

            }

            try

            {

                var createdNote = await _noteRepository.AddNoteAsync(dto);

                _logger.LogInformation($"Note created successfully with ID {createdNote.Id}");

                return CreatedAtAction(nameof(GetNote), new { id = createdNote.Id }, createdNote);

            }

            catch (System.Exception ex)

            {

                _logger.LogError(ex, "Error creating note");

                return StatusCode(500, "An error occurred while creating the note");

            }

        }

        [HttpGet("{id}")]

        [ProducesResponseType(typeof(NoteReadDto), 200)]

        [ProducesResponseType(404)]

        public async Task<ActionResult<NoteReadDto>> GetNote(long id)

        {

            _logger.LogInformation($"Fetching note with ID {id}");

            var note = await _noteRepository.GetNoteByIdAsync(id);

            if (note == null)

            {

                _logger.LogWarning($"Note with ID {id} not found");

                return NotFound($"Note with ID {id} not found.");

            }

            return Ok(note);

        }

        [HttpGet("deal/{dealId}")]

        [ProducesResponseType(typeof(IEnumerable<NoteReadDto>), 200)]

        [ProducesResponseType(403)]

        public async Task<ActionResult<IEnumerable<NoteReadDto>>> GetNotesByDeal(long dealId, [FromQuery] long userId)

        {

            _logger.LogInformation($"User {userId} fetching notes for deal {dealId}");

            // Check privileges

            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync((int)userId);

            if (!privileges.Any(p => p.PrivilegeName == "ViewNotes" || p.PrivilegeName == "PipelineDetailAccess"))

            {

                _logger.LogWarning($"User {userId} lacks ViewNotes privilege");

                return Forbid("User lacks ViewNotes privilege");

            }

            try

            {

                var notes = await _noteRepository.GetNotesByDealIdAsync(dealId);

                _logger.LogInformation($"Found {notes.Count()} notes for deal {dealId}");

                return Ok(notes);

            }

            catch (System.Exception ex)

            {

                _logger.LogError(ex, $"Error fetching notes for deal {dealId}");

                return StatusCode(500, "An error occurred while fetching notes");

            }

        }

        [HttpPut("{id}")]

        [ProducesResponseType(typeof(NoteReadDto), 200)]

        [ProducesResponseType(404)]

        [ProducesResponseType(403)]

        public async Task<ActionResult<NoteReadDto>> UpdateNote(long id, [FromBody] NoteUpdateDto dto)

        {

            _logger.LogInformation($"User {dto.UpdatedBy} updating note {id}");

            // Check privileges

            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync((int)dto.UpdatedBy);

            if (!privileges.Any(p => p.PrivilegeName == "UpdateNote" || p.PrivilegeName == "PipelineDetailAccess"))

            {

                _logger.LogWarning($"User {dto.UpdatedBy} lacks UpdateNote privilege");

                return Forbid("User lacks UpdateNote privilege");

            }

            if (!ModelState.IsValid)

            {

                return BadRequest(ModelState);

            }

            try

            {

                var updatedNote = await _noteRepository.UpdateNoteAsync(id, dto);

                if (updatedNote == null)

                {

                    _logger.LogWarning($"Note with ID {id} not found for update");

                    return NotFound($"Note with ID {id} not found.");

                }

                _logger.LogInformation($"Note {id} updated successfully");

                return Ok(updatedNote);

            }

            catch (System.Exception ex)

            {

                _logger.LogError(ex, $"Error updating note {id}");

                return StatusCode(500, "An error occurred while updating the note");

            }

        }

        [HttpDelete("{id}")]

        [ProducesResponseType(204)]

        [ProducesResponseType(404)]

        [ProducesResponseType(403)]

        public async Task<IActionResult> DeleteNote(long id, [FromQuery] long userId)

        {

            _logger.LogInformation($"User {userId} deleting note {id}");

            // Check privileges

            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync((int)userId);

            if (!privileges.Any(p => p.PrivilegeName == "DeleteNote" || p.PrivilegeName == "PipelineDetailAccess"))

            {

                _logger.LogWarning($"User {userId} lacks DeleteNote privilege");

                return Forbid("User lacks DeleteNote privilege");

            }

            try

            {

                var result = await _noteRepository.DeleteNoteAsync(id);

                if (!result)

                {

                    _logger.LogWarning($"Note with ID {id} not found for deletion");

                    return NotFound($"Note with ID {id} not found.");

                }

                _logger.LogInformation($"Note {id} deleted successfully");

                return NoContent();

            }

            catch (System.Exception ex)

            {

                _logger.LogError(ex, $"Error deleting note {id}");

                return StatusCode(500, "An error occurred while deleting the note");

            }

        }

        // Test endpoint to verify controller is loaded

        [HttpGet("test")]

        public IActionResult Test()

        {

            _logger.LogInformation("Notes controller test endpoint called");

            return Ok(new { message = "Notes controller is working", timestamp = System.DateTime.UtcNow });

        }

    }

}
