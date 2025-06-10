using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using Microsoft.Extensions.Options;
using XLead_Server.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph.Models.Security;
using Microsoft.SqlServer.Dac.Model;
using System.Reflection.Emit;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiQueryController : ControllerBase
    {
        private readonly IDbSchemaRepository _schemaService;
        private readonly IAiQueryGeneratorRepository _aiQueryGenerator;
        private readonly ISqlValidationService _sqlValidationService;
        private readonly IDataQueryRepository _dataQueryService;
        private readonly ILogger<AiQueryController> _logger;
        private readonly GeminiSettings _geminiSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public AiQueryController(
            IDbSchemaRepository schemaService,
            IAiQueryGeneratorRepository aiQueryGenerator,
            ISqlValidationService sqlValidationService,
            IDataQueryRepository dataQueryService,
            ILogger<AiQueryController> logger,
            IOptions<GeminiSettings> geminiSettings,
            IHttpClientFactory httpClientFactory)
        {
            _schemaService = schemaService;
            _aiQueryGenerator = aiQueryGenerator;
            _sqlValidationService = sqlValidationService;
            _dataQueryService = dataQueryService;
            _logger = logger;
            _geminiSettings = geminiSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("process-natural-language")]
        public async Task<ActionResult<AiQueryResponseDto>> ProcessNaturalLanguageQuery([FromBody] AiQueryRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Received natural language query: {Query}", request.NaturalLanguageQuery);

            try
            {
                _logger.LogInformation("Fetching database schema...");
                var dbSchema = await _schemaService.GetFormattedSchemaAsync();
                if (string.IsNullOrWhiteSpace(dbSchema))
                {
                    _logger.LogError("Database schema could not be retrieved or is empty.");
                    return StatusCode(500, new AiQueryResponseDto { Success = false, Message = "Internal error: Could not retrieve database schema." });
                }

                _logger.LogInformation("Generating SQL query via AI...");
                var generatedSql = await _aiQueryGenerator.GenerateSqlQueryAsync(request.NaturalLanguageQuery, dbSchema);
                _logger.LogInformation("AI Raw Generated SQL: {Sql}", generatedSql);

                if (string.IsNullOrWhiteSpace(generatedSql) || generatedSql.StartsWith("ERROR_") || generatedSql.Equals("AMBIGUOUS_OR_UNSUPPORTED", StringComparison.OrdinalIgnoreCase))
                {
                    string userMessage = "I'm sorry, I couldn't understand that query. It might be too ambiguous, or beyond my current capabilities.";
                    if (generatedSql != null && generatedSql.Contains("Status 429")) userMessage = "The AI service is currently busy. Please try again in a moment.";
                    else if (generatedSql != null && generatedSql.StartsWith("ERROR_GEMINI_API")) userMessage = "There was an issue communicating with the AI service. The configured model might be unavailable.";
                    else if (generatedSql != null && generatedSql.StartsWith("ERROR_")) userMessage = "An internal error occurred while processing the AI request.";

                    _logger.LogWarning("AI could not generate a valid SQL query or indicated an API error. AI Output: {Output}", generatedSql);
                    return Ok(new AiQueryResponseDto { Success = false, Message = userMessage });
                }

                _logger.LogInformation("Validating generated SQL: {Sql}", generatedSql);
                var validationResult = _sqlValidationService.ValidateQuery(generatedSql, out string validationMessage);
                if (validationResult != SqlValidationResult.Safe)
                {
                    _logger.LogWarning("Generated SQL failed validation. Reason: {Reason}, SQL: {Sql}", validationMessage, generatedSql);
                    return Ok(new AiQueryResponseDto { Success = false, Message = $"The generated query was deemed unsafe or invalid: {validationMessage}", GeneratedSql = generatedSql });
                }

                _logger.LogInformation("Executing validated SQL query...");
                var queryResult = await _dataQueryService.ExecuteSelectQueryAsync(generatedSql);

                if (!queryResult.Success)
                {
                    _logger.LogWarning("SQL execution failed. Error: {Error}, SQL: {Sql}", queryResult.ErrorMessage, generatedSql);
                    return Ok(new AiQueryResponseDto { Success = false, Message = $"Error executing query: {queryResult.ErrorMessage}", GeneratedSql = generatedSql });
                }

                _logger.LogInformation("Query executed successfully. {Count} records found.", queryResult.RecordsAffected);

                string finalMessage;
                if (queryResult.RecordsAffected > 0 && queryResult.Data != null && queryResult.Data.Any())
                {
                    var dataSample = queryResult.Data.Take(5).ToList();

                    var anonymizedSample = AnonymizeDataWithPlaceholders(dataSample);
                    var jsonDataSample = JsonSerializer.Serialize(anonymizedSample, new JsonSerializerOptions { WriteIndented = true });

                    _logger.LogInformation("Requesting AI to summarize anonymized data sample with placeholders.");

                    string summaryTemplate = await _aiQueryGenerator.SummarizeDataAsync(
                        request.NaturalLanguageQuery,
                        jsonDataSample,
                        anonymizedSample.Count,
                        queryResult.RecordsAffected
                    );

                    if (!string.IsNullOrWhiteSpace(summaryTemplate))
                    {
                        finalMessage = RehydrateSummary(summaryTemplate, dataSample);
                    }
                    else
                    {
                        finalMessage = $"Successfully retrieved {queryResult.RecordsAffected} record(s). A user-friendly summary could not be generated at this time.";
                    }
                }
                else
                {
                    finalMessage = "I ran the query successfully, but no matching records were found.";
                }

                return Ok(new AiQueryResponseDto
                {
                    Success = true,
                    Message = finalMessage,
                    Results = queryResult.Data,
                    GeneratedSql = generatedSql,
                    Count = queryResult.RecordsAffected
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in ProcessNaturalLanguageQuery for query: {Query}", request.NaturalLanguageQuery);
                return StatusCode(500, new AiQueryResponseDto { Success = false, Message = "An unexpected internal server error occurred." });
            }
        }

        private List<Dictionary<string, object>> AnonymizeDataWithPlaceholders(List<Dictionary<string, object>> data)
        {
            var piiColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                //"FirstName", "LastName", "Email", "PhoneNumber", "ContactName", "Address", "Description",
                //    // Business-specific proprietary data
                //"DealName", "CustomerName", "CompanyName", "AccountName", "ProjectName",

                //// Financial Data
                //"DealAmount", "Amount", "Price", "Salary", "Revenue",

                //// Free-text fields that might contain sensitive info
                //"Description", "Notes", "Comment", "CustomFields",

                //// Internal IDs that you might want to hide
                //"ContactId", "AccountId", "UserId", "CreatedBy", "UpdatedBy"
            };

            var anonymizedData = new List<Dictionary<string, object>>();
            for (int i = 0; i < data.Count; i++)
            {
                var originalRow = data[i];
                var newRow = new Dictionary<string, object>();
                foreach (var kvp in originalRow)
                {
                    if (piiColumns.Contains(kvp.Key))
                    {
                        newRow[kvp.Key] = $"{{{{RECORD_{i}_{kvp.Key.ToUpper()}}}}}";
                    }
                    else
                    {
                        newRow[kvp.Key] = kvp.Value;
                    }
                }
                anonymizedData.Add(newRow);
            }
            return anonymizedData;
        }

        private string RehydrateSummary(string summaryTemplate, List<Dictionary<string, object>> originalData)
        {
            var piiColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                
               " FirstName", "LastName", "Email", "PhoneNumber", "ContactName", "Address", "City", "State", "ZipCode",

                // Business-specific proprietary data
                "DealName", "CustomerName", "CompanyName", "AccountName", "ProjectName",

                // Financial Data
                "DealAmount", "Amount", "Price", "Salary", "Revenue",

                // Free-text fields that might contain sensitive info
                "Description", "Notes", "Comment", "CustomFields",

                // Internal IDs that you might want to hide
                "ContactId", "AccountId", "UserId", "CreatedBy", "UpdatedBy"
            };

            var sb = new StringBuilder(summaryTemplate);
            for (int i = 0; i < originalData.Count; i++)
            {
                var originalRow = originalData[i];
                foreach (var kvp in originalRow)
                {
                    if (piiColumns.Contains(kvp.Key))
                    {
                        string placeholder = $"{{{{RECORD_{i}_{kvp.Key.ToUpper()}}}}}";
                        string originalValue = kvp.Value?.ToString() ?? "";
                        sb.Replace(placeholder, originalValue);
                    }
                }
            }
            return sb.ToString();
        }
    }
}