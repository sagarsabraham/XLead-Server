// XLead_Server/Services/AiQueryGeneratorRepository.cs
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using XLead_Server.Configuration;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;

namespace XLead_Server.Services
{
    public class AiQueryGeneratorRepository : IAiQueryGeneratorRepository
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _geminiSettings;
        private readonly ILogger<AiQueryGeneratorRepository> _logger;

        public AiQueryGeneratorRepository(
            IHttpClientFactory httpClientFactory,
            IOptions<GeminiSettings> geminiSettingsOptions,
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
        }

        public async Task<string> GenerateSqlQueryAsync(string naturalLanguageQuery, string dbSchema)
        {
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

---

Natural Language Query: ""{naturalLanguageQuery}""

SQL Query:";

            var requestPayload = new GeminiRequestDto
            {
                Contents = new List<GeminiContentDto>
                {
                    new GeminiContentDto { Parts = new List<GeminiPartDto> { new GeminiPartDto { Text = fullPrompt } } }
                },
                GenerationConfig = new GeminiGenerationConfigDto
                {
                    Temperature = _geminiSettings.Temperature,
                    MaxOutputTokens = _geminiSettings.MaxOutputTokens
                }
            };

            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiSettings.Model}:generateContent?key={_geminiSettings.ApiKey}";
            _logger.LogInformation("Sending request to Gemini API for SQL generation. Model: {Model}", _geminiSettings.Model);

            try
            {
                using (var response = await _httpClient.PostAsJsonAsync(apiUrl, requestPayload))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Error calling Gemini API for SQL generation. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                        return $"ERROR_GEMINI_API: Status {response.StatusCode}";
                    }

                    var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponseDto>();
                    var generatedSql = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Trim();

                    if (generatedSql != null)
                    {
                        if (generatedSql.StartsWith("```sql", StringComparison.OrdinalIgnoreCase)) generatedSql = generatedSql.Substring(5).TrimStart();
                        if (generatedSql.EndsWith("```")) generatedSql = generatedSql.Substring(0, generatedSql.Length - 3).TrimEnd();
                        generatedSql = generatedSql.Replace(";", "").Trim();
                    }

                    return string.IsNullOrWhiteSpace(generatedSql) ? "AMBIGUOUS_OR_UNSUPPORTED" : generatedSql;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in GenerateSqlQueryAsync.");
                return "ERROR_UNEXPECTED";
            }
        }

        public async Task<string> SummarizeDataAsync(string originalQuery, string jsonData, int sampleCount, int totalRecordCount)
        {
            var prompt = $@"You are a helpful and clever data analyst assistant. Your task is to transform raw JSON data into a beautiful, human-readable summary for a chatbot.

**Core Instructions:**
- **Be Conversational:** Start your response naturally.
- **Synthesize, Don't List:** Do not just dump fields. Pick 2-3 of the most important fields and weave them into a sentence or a clean markdown list.
- **Understand Intent:** Use the 'Original User Query' to figure out what is most important to show.
- **Use Markdown:** Use lists (`- `) for multiple items. Use bold (`**text**`) for emphasis.
- **Handle Counts:** If the query was a 'how many' question, the primary answer is the total count.
- **Acknowledge More Data:** If the total record count is greater than the sample count you were given, mention it at the end (e.g., ""...and X more records were found."").

---
**Example 1: List of Deals**
*   **Context:** User asked 'show me deals created this week'. You are given a sample of 2 records out of a total of 7.
*   **JSON Data Sample:** `[ {{ 'DealName': 'Website Redesign', 'DealAmount': 5000 }}, {{ 'DealName': 'API Integration', 'DealAmount': 12000 }} ]`
*   **A+ Example Summary:**
    Of course! I found 7 deals created this week. Here are the first few:
    - **Website Redesign** (valued at $5,000.00)
    - **API Integration** (valued at $12,000.00)
    ...and 5 more records were found.

---
**Example 2: A 'Count' Question**
*   **Context:** User asked 'how many deals are in the closed won stage?'. You are given a sample of 1 record out of a total of 1.
*   **JSON Data Sample:** `[ {{ 'count': 9 }} ]`
*   **A+ Example Summary:**
    You have a total of **9 deals** in the 'Closed Won' stage. Great job!

---
**YOUR TASK**

*   **Original User Query:** ""{originalQuery}""
*   **Total Records Found:** {totalRecordCount}
*   **Sample Records Provided:** {sampleCount}
*   **JSON Data Sample:**
    {jsonData}
---

**Your Summary:**";

            var requestPayload = new GeminiRequestDto
            {
                Contents = new List<GeminiContentDto>
                {
                    new GeminiContentDto { Parts = new List<GeminiPartDto> { new GeminiPartDto { Text = prompt } } }
                },
                GenerationConfig = new GeminiGenerationConfigDto
                {
                    Temperature = 0.4,
                    MaxOutputTokens = 500
                }
            };

            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiSettings.Model}:generateContent?key={_geminiSettings.ApiKey}";
            _logger.LogInformation("Sending data to Gemini for summarization with corrected prompt examples.");

            try
            {
                using (var response = await _httpClient.PostAsJsonAsync(apiUrl, requestPayload))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Error calling Gemini API for summarization. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                        return null;
                    }

                    var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponseDto>();
                    return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception during data summarization.");
                return null;
            }
        }
    }
}