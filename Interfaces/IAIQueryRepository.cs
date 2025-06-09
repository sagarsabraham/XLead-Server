using System.Threading.Tasks;

namespace XLead_Server.Interfaces
{
    public interface IAiQueryGeneratorRepository
    {
        Task<string> GenerateSqlQueryAsync(string naturalLanguageQuery, string dbSchema);
    }
}