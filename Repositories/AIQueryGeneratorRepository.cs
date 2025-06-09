// XLead_Server/Services/AiQueryGeneratorService.cs
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using XLead_Server.Configuration;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace XLead_Server.Services
{
    public class AiQueryGeneratorRepository : IAiQueryGeneratorRepository
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _openAiSettings;
        private readonly ILogger<AiQueryGeneratorRepository> _logger;

        public AiQueryGeneratorRepository(
            IHttpClientFactory httpClientFactory,
            IOptions<OpenAISettings> openAiSettingsOptions,
            ILogger<AiQueryGeneratorRepository> logger)
        {
            _httpClient = httpClientFactory.CreateClient("OpenAIClient");
            _openAiSettings = openAiSettingsOptions.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_openAiSettings.ApiKey))
            {
                _logger.LogError("OpenAI API Key is not configured.");
                throw new InvalidOperationException("OpenAI API Key is not configured.");
            }
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);
        }

        public async Task<string> GenerateSqlQueryAsync(string naturalLanguageQuery, string dbSchema)
        {
            // Improved prompt engineering
            var systemMessage = $@"You are an expert SQL generation AI. Your task is to translate natural language queries into valid SQL Server SELECT statements.
Database Schema:
--- SCHEMA START ---
{dbSchema}
--- SCHEMA END ---

Constraints & Guidelines:
1. ONLY generate a single, valid SQL SELECT statement.
2. DO NOT use DML (INSERT, UPDATE, DELETE, MERGE, TRUNCATE) or DDL (CREATE, ALTER, DROP).
3. DO NOT include any explanations, comments (like -- or /* */), or markdown (like ```sql).
4. If the query is ambiguous, cannot be answered, or requires modification capabilities, output: AMBIGUOUS_OR_UNSUPPORTED
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

User: ""How many deals were closed last month?""
SQL Query:
SELECT COUNT(*) FROM Deals WHERE Stage = 'Closed Won' AND CloseDate >= DATEADD(month, DATEDIFF(month, 0, GETDATE())-1, 0) AND CloseDate < DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0);";

            var userPrompt = $"Natural Language Query: \"{naturalLanguageQuery}\"\n\nSQL Query:";

            var requestPayload = new OpenAiChatCompletionRequestDto
            {
                Model = _openAiSettings.Model,
                Messages = new List<OpenAiMessageDto>
                {
                    new OpenAiMessageDto { Role = "system", Content = systemMessage },
                    new OpenAiMessageDto { Role = "user", Content = userPrompt }
                },
                Temperature = _openAiSettings.Temperature,
                MaxTokens = _openAiSettings.MaxTokens
            };

            _logger.LogInformation("Sending request to OpenAI. Model: {Model}, Endpoint: {Endpoint}", _openAiSettings.Model, _openAiSettings.ApiEndpoint);

            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsJsonAsync(_openAiSettings.ApiEndpoint, requestPayload);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error calling OpenAI API. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                    // Consider throwing a custom exception or returning a specific error code
                    return $"ERROR_OPENAI_API: Status {response.StatusCode}";
                }

                var openAiResponse = await response.Content.ReadFromJsonAsync<OpenAiChatCompletionResponseDto>();
                var generatedSql = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

                _logger.LogInformation("OpenAI Raw Response: {Sql}", generatedSql);

                // Clean up potential markdown if AI doesn't strictly follow instructions
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
                    generatedSql = generatedSql.Replace(";", "").Trim(); // Remove trailing semicolons
                }


                if (string.IsNullOrWhiteSpace(generatedSql))
                {
                    _logger.LogWarning("OpenAI returned an empty or whitespace SQL query.");
                    return "AMBIGUOUS_OR_UNSUPPORTED"; // Or a more specific error
                }

                return generatedSql;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception while calling OpenAI API.");
                return "ERROR_HTTP_REQUEST";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization exception for OpenAI response.");
                return "ERROR_JSON_DESERIALIZATION";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in AiQueryGeneratorService.");
                return "ERROR_UNEXPECTED";
            }
            finally
            {
                response?.Dispose();
            }
        }
    }
}