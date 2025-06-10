using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class ApiResponseWrapperFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var objectResult = context.Result as ObjectResult;

            if (objectResult == null)
            {
                // If result is not an ObjectResult (could be StatusCodeResult, EmptyResult), 
                // you can optionally wrap it or just continue
                await next();
                return;
            }

            // Prevent double wrapping: if already wrapped, skip
            var type = objectResult.Value?.GetType();
            if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                await next();
                return;
            }

            // Prepare wrapped response
            var statusCode = objectResult.StatusCode ?? 200;

            var apiResponseType = typeof(ApiResponse<>).MakeGenericType(type ?? typeof(object));

            // Create ApiResponse instance dynamically
            var apiResponse = Activator.CreateInstance(apiResponseType, objectResult.Value, statusCode, null, null);

            context.Result = new ObjectResult(apiResponse)
            {
                StatusCode = statusCode
            };

            await next();
        }
    }
}