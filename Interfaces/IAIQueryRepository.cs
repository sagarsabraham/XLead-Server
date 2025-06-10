using System.Threading.Tasks;

namespace XLead_Server.Interfaces
{
    public interface IAiQueryGeneratorRepository
    {
        Task<string> GenerateSqlQueryAsync(string naturalLanguageQuery, string dbSchema);
        Task<string> SummarizeDataAsync(string originalQuery, string jsonData, int sampleCount, int totalRecordCount);
    }
}