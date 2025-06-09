// XLead_Server/Interfaces/IDataQueryService.cs
using System.Threading.Tasks;
using XLead_Server.DTOs;

namespace XLead_Server.Interfaces
{
    public interface IDataQueryRepository
    {
        Task<QueryResultDto> ExecuteSelectQueryAsync(string sqlQuery);
    }
}