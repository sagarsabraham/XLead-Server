using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;

namespace XLead_Server.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }

        [JsonConstructor]
        public ApiResponse() { }

        public ApiResponse(T? data, int statusCode, string? message = null, object? meta = null)
        {
            Data = data;
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode) ?? string.Empty;
            Success = statusCode >= 200 && statusCode < 300;
        }

        private static string? GetDefaultMessageForStatusCode(int statusCode)
        {
            return ReasonPhrases.GetReasonPhrase(statusCode);
        }
    }
}