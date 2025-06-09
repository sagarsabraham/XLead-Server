using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XLead_Server.DTOs
{
    public class AiQueryRequestDto
    {
        [Required]
        [MinLength(3)]
        public string NaturalLanguageQuery { get; set; }
    }

    public class AiQueryResponseDto
    {
        public string Message { get; set; }
        public List<Dictionary<string, object>> Results { get; set; }
        public string GeneratedSql { get; set; }
        public int? Count { get; set; }
        public bool Success { get; set; } = true; // Default to true, set to false on error
    }

    public class QueryResultDto // Used by IDataQueryService
    {
        public bool Success { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }
        public string ErrorMessage { get; set; }
        public int RecordsAffected { get; set; }
    }
}