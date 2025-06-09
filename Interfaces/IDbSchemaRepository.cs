// XLead_Server/Interfaces/IDbSchemaService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.DTOs;

namespace XLead_Server.Interfaces
{
    public interface IDbSchemaRepository
    {
        Task<string> GetFormattedSchemaAsync();
        Task<IEnumerable<TableSchemaDto>> GetDetailedSchemaStructureAsync();
    }
}