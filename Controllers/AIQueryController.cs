// XLead_Server/Controllers/AiQueryController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using Microsoft.Extensions.Options;      // Add this using
using XLead_Server.Configuration;      // Add this using
using System.Net.Http;                   // Add this using

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
        private readonly GeminiSettings _geminiSettings;         // Add this field for the new endpoint
        private readonly IHttpClientFactory _httpClientFactory;  // Add this field for the new endpoint

        public AiQueryController(
            IDbSchemaRepository schemaService,
            IAiQueryGeneratorRepository aiQueryGenerator,
            ISqlValidationService sqlValidationService,
            IDataQueryRepository dataQueryService,
            ILogger<AiQueryController> logger,
            IOptions<GeminiSettings> geminiSettings, // Add this to the constructor
            IHttpClientFactory httpClientFactory)    // Add this to the constructor
        {
            _schemaService = schemaService;
            _aiQueryGenerator = aiQueryGenerator;
            _sqlValidationService = sqlValidationService;
            _dataQueryService = dataQueryService;
            _logger = logger;
            _geminiSettings = geminiSettings.Value;      // Store the injected settings
            _httpClientFactory = httpClientFactory;      // Store the injected factory
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
                _logger.LogInformation("Database schema fetched successfully.");

                _logger.LogInformation("Generating SQL query via AI...");
                var generatedSql = await _aiQueryGenerator.GenerateSqlQueryAsync(request.NaturalLanguageQuery, dbSchema);
                _logger.LogInformation("AI Raw Generated SQL: {Sql}", generatedSql);


                if (string.IsNullOrWhiteSpace(generatedSql) ||
                    generatedSql.StartsWith("ERROR_") ||
                    generatedSql.Equals("AMBIGUOUS_OR_UNSUPPORTED", StringComparison.OrdinalIgnoreCase))
                {
                    string userMessage = "I'm sorry, I couldn't understand that query, it was too ambiguous, or it's beyond my current capabilities.";

                    if (generatedSql != null && generatedSql.Contains("Status 429")) // Handle rate limiting
                    {
                        userMessage = "The AI service is currently busy due to high demand. Please try again in a moment.";
                    }
                    else if (generatedSql != null && (generatedSql.StartsWith("ERROR_GEMINI_API") || generatedSql.StartsWith("ERROR_HTTP_REQUEST")))
                    {
                        userMessage = "There was an issue communicating with the AI service. The configured model might be unavailable.";
                    }
                    else if (generatedSql != null && generatedSql.StartsWith("ERROR_"))
                    {
                        userMessage = "An internal error occurred while processing the AI request.";
                    }

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
                _logger.LogInformation("SQL validation successful.");

                _logger.LogInformation("Executing validated SQL query...");
                var queryResult = await _dataQueryService.ExecuteSelectQueryAsync(generatedSql);

                if (!queryResult.Success)
                {
                    _logger.LogWarning("SQL execution failed. Error: {Error}, SQL: {Sql}", queryResult.ErrorMessage, generatedSql);
                    return Ok(new AiQueryResponseDto { Success = false, Message = $"Error executing query: {queryResult.ErrorMessage}", GeneratedSql = generatedSql });
                }

                _logger.LogInformation("Query executed successfully. {Count} records found.", queryResult.RecordsAffected);
                return Ok(new AiQueryResponseDto
                {
                    Success = true,
                    Message = $"Successfully retrieved {queryResult.RecordsAffected} record(s).",
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

        // --- THIS IS THE NEW DIAGNOSTIC ENDPOINT ---
        [HttpGet("list-available-models")]
        public async Task<IActionResult> ListAvailableModels()
        {
            if (string.IsNullOrWhiteSpace(_geminiSettings.ApiKey))
            {
                return BadRequest("Gemini API Key is not configured in appsettings.json.");
            }

            var client = _httpClientFactory.CreateClient("GeminiClient");
            var listModelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={_geminiSettings.ApiKey}";

            try
            {
                _logger.LogInformation("Requesting list of available models from Google AI.");
                var response = await client.GetAsync(listModelsUrl);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to list models. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, content);
                    return StatusCode((int)response.StatusCode, new { message = "Failed to list models.", details = content });
                }

                // Return the successful response as raw JSON, which is what we need to inspect.
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while trying to list available models.");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}