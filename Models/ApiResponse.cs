namespace XLead_Server.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public ApiResponse(bool success, string? message = null, List<string>? errors = null)
        {
            Success = success;
            Message = message;
            Errors = errors;
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public ApiResponse(bool success, T? data, string? message = null, List<string>? errors = null)
            : base(success, message, errors)
        {
            Data = data;
        }
    }
}
}
