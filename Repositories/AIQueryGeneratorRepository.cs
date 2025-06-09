// XLead_Server/Services/AiQueryGeneratorRepository.cs
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using XLead_Server.Configuration; // IMPORTANT: Change this from OpenAISettings
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace XLead_Server.Services
{
    public class AiQueryGeneratorRepository : IAiQueryGeneratorRepository
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _geminiSettings; // Use GeminiSettings
        private readonly ILogger<AiQueryGeneratorRepository> _logger;

        public AiQueryGeneratorRepository(
            IHttpClientFactory httpClientFactory,
            IOptions<GeminiSettings> geminiSettingsOptions, // Inject GeminiSettings
            ILogger<AiQueryGeneratorRepository> logger)
        {
            _httpClient = httpClientFactory.CreateClient("GeminiClient");
            _geminiSettings = geminiSettingsOptions.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_geminiSettings.ApiKey))
            {
                _logger.LogError("Gemini API Key is not configured.");
                throw new InvalidOperationException("Gemini API Key is not configured.");
            }

            // NOTE: The Gemini API key is NOT sent as a Bearer token.
            // It's sent as a query parameter in the URL. So we remove the old header logic.
        }

        public async Task<string> GenerateSqlQueryAsync(string naturalLanguageQuery, string dbSchema)
        {
            // The prompt remains largely the same, but Gemini doesn't use a separate "system" role.
            // We combine everything into one text block.
            var fullPrompt = $@"You are an expert SQL generation AI. Your task is to translate natural language queries into valid SQL Server SELECT statements.
Database Schema:
--- SCHEMA START ---
{dbSchema}
--- SCHEMA END ---

Constraints & Guidelines:
1. ONLY generate a single, valid SQL SELECT statement.
2. DO NOT use DML (INSERT, UPDATE, DELETE, MERGE, TRUNCATE) or DDL (CREATE, ALTER, DROP).
3. DO NOT include any explanations, comments (like -- or /* */), or markdown (like ```sql).
4. If the query is ambiguous, cannot be answered, or requires modification capabilities, output the single word: AMBIGUOUS_OR_UNSUPPORTED
5. Prioritize clarity and correctness. Use table aliases if it improves readability for complex joins.
6. For date queries, assume 'today' is {DateTime.UtcNow:yyyy-MM-dd}. Interpret relative dates like 'last month', 'this week' (Mon-Sun) appropriately.
7. Ensure all column names and table names used exist in the provided schema.
8. Do not hallucinate columns or tables. If a concept isn't in the schema, it cannot be queried.
9. Be careful with aggregate functions (COUNT, SUM, AVG) and GROUP BY clauses. Only use them if explicitly asked or strongly implied for summarization.
10. If the user asks for 'top N' or 'bottom N', use TOP N ... ORDER BY.
11. For text searches, use LIKE with wildcards (e.g., '%term%') if a partial match is implied.
Example:
User: ""Show me all contacts in London""
SQL Query:
SELECT * FROM Contacts WHERE City = 'London';

Example:
User: ""How many deals were closed last month?""
SQL Query:
SELECT COUNT(*) FROM Deals WHERE Stage = 'Closed Won' AND CloseDate >= DATEADD(month, DATEDIFF(month, 0, GETDATE())-1, 0) AND CloseDate < DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0);

---

Natural Language Query: ""{naturalLanguageQuery}""

SQL Query:";

            var requestPayload = new GeminiRequestDto
            {
                Contents = new List<GeminiContentDto>
                {
                    new GeminiContentDto
                    {
                        Parts = new List<GeminiPartDto> { new GeminiPartDto { Text = fullPrompt } }
                    }
                },
                GenerationConfig = new GeminiGenerationConfigDto
                {
                    Temperature = _geminiSettings.Temperature,
                    MaxOutputTokens = _geminiSettings.MaxOutputTokens
                }
            };

            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiSettings.Model}:generateContent?key={_geminiSettings.ApiKey}";

            _logger.LogInformation("Sending request to Gemini API. Model: {Model}, URL: {ApiUrl}", _geminiSettings.Model, apiUrl);
            _logger.LogInformation("Sending request to Gemini API. Model: {Model}, URL: {ApiUrl}", _geminiSettings.Model, apiUrl);

            _logger.LogInformation("Sending request to Gemini API. Model: {Model}", _geminiSettings.Model);

            try
            {
                using (var response = await _httpClient.PostAsJsonAsync(apiUrl, requestPayload))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Error calling Gemini API. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                        return $"ERROR_GEMINI_API: Status {response.StatusCode}";
                    }

                    var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponseDto>();

                    // The generated text is nested deeper in the Gemini response
                    var generatedSql = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Trim();

                    _logger.LogInformation("Gemini Raw Response: {Sql}", generatedSql);

                    // The cleanup logic for markdown and semicolons is still useful
                    if (generatedSql != null)
                    {
                        if (generatedSql.StartsWith("```sql", StringComparison.OrdinalIgnoreCase))
                        {
                            generatedSql = generatedSql.Substring(5).TrimStart();
                        }
                        if (generatedSql.EndsWith("```"))
                        {
                            generatedSql = generatedSql.Substring(0, generatedSql.Length - 3).TrimEnd();
                        }
                        generatedSql = generatedSql.Replace(";", "").Trim();
                    }

                    if (string.IsNullOrWhiteSpace(generatedSql))
                    {
                        _logger.LogWarning("Gemini returned an empty or whitespace SQL query.");
                        return "AMBIGUOUS_OR_UNSUPPORTED";
                    }

                    return generatedSql;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in AiQueryGeneratorRepository (Gemini).");
                // Custom error codes for easier debugging
                if (ex is System.Text.Json.JsonException) return "ERROR_JSON_DESERIALIZATION";
                if (ex is HttpRequestException) return "ERROR_HTTP_REQUEST";
                return "ERROR_UNEXPECTED";
            }
        }
    }
}