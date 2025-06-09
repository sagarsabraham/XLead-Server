using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;
namespace XLead_Server.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly ApiDbContext _context;
        private readonly ILogger<NoteRepository> _logger;
        public NoteRepository(ApiDbContext context, ILogger<NoteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<NoteReadDto> AddNoteAsync(NoteCreateDto noteDto)
        {
            try
            {
                var note = new Note
                {
                    NoteText = noteDto.NoteText,
                    DealId = noteDto.DealId,
                    CreatedBy = noteDto.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notes.Add(note);
                await _context.SaveChangesAsync();
                return await GetNoteByIdAsync(note.Id);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding note");
                throw;
            }
        }

        public async Task<IEnumerable<NoteReadDto>> GetNotesByDealIdAsync(long dealId)

        {

            try

            {

                var notes = await _context.Notes

                    .Include(n => n.Creator)

                    .Include(n => n.Updater)

                    .Where(n => n.DealId == dealId)

                    .OrderByDescending(n => n.CreatedAt)

                    .Select(n => new NoteReadDto

                    {

                        Id = n.Id,

                        NoteText = n.NoteText,

                        DealId = n.DealId,

                        CreatedBy = n.CreatedBy,

                        CreatedByName = n.Creator.Name ?? "Unknown User",

                        CreatedAt = n.CreatedAt,

                        UpdatedBy = n.UpdatedBy,

                        UpdatedByName = n.Updater != null ? n.Updater.Name : null,

                        UpdatedAt = n.UpdatedAt

                    })

                    .ToListAsync();

                return notes;

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, $"Error getting notes for deal {dealId}");

                throw;

            }

        }

        public async Task<NoteReadDto> GetNoteByIdAsync(long id)

        {

            try

            {

                var note = await _context.Notes

                    .Include(n => n.Creator)

                    .Include(n => n.Updater)

                    .Where(n => n.Id == id)

                    .Select(n => new NoteReadDto

                    {

                        Id = n.Id,

                        NoteText = n.NoteText,

                        DealId = n.DealId,

                        CreatedBy = n.CreatedBy,

                        CreatedByName = n.Creator.Name ?? "Unknown User",

                        CreatedAt = n.CreatedAt,

                        UpdatedBy = n.UpdatedBy,

                        UpdatedByName = n.Updater != null ? n.Updater.Name : null,

                        UpdatedAt = n.UpdatedAt

                    })

                    .FirstOrDefaultAsync();

                return note;

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, $"Error getting note {id}");

                throw;

            }

        }

        public async Task<NoteReadDto> UpdateNoteAsync(long id, NoteUpdateDto noteDto)

        {

            try

            {

                var note = await _context.Notes

                    .FirstOrDefaultAsync(n => n.Id == id);

                if (note == null)

                    return null;

                note.NoteText = noteDto.NoteText;

                note.UpdatedBy = noteDto.UpdatedBy;

                note.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetNoteByIdAsync(id);

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, $"Error updating note {id}");

                throw;

            }

        }

        public async Task<bool> DeleteNoteAsync(long id)

        {

            try

            {

                var note = await _context.Notes

                    .FirstOrDefaultAsync(n => n.Id == id);

                if (note == null)

                    return false;

                _context.Notes.Remove(note);

                await _context.SaveChangesAsync();

                return true;

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, $"Error deleting note {id}");

                throw;

            }

        }

    }

}
