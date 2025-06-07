using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.DTOs;

namespace XLead_Server.Interfaces
{
    public interface INoteRepository
    {
        Task<NoteReadDto> AddNoteAsync(NoteCreateDto noteDto);
        Task<IEnumerable<NoteReadDto>> GetNotesByDealIdAsync(long dealId);
        Task<NoteReadDto> GetNoteByIdAsync(long id);
        Task<NoteReadDto> UpdateNoteAsync(long id, NoteUpdateDto noteDto);
        Task<bool> DeleteNoteAsync(long id);
    }
}