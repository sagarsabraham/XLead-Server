//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.AspNetCore.Mvc;

//namespace XLead_Server.Shared
//{
//    public class ApiResponseFilter : IActionFilter
//    {
//        public void OnActionExecuting(ActionExecutingContext context)
//        {
//            // No action needed before the action executes
//        }

//        public void OnActionExecuted(ActionExecutedContext context)
//        {
//            if (context.Result == null)
//                return;

//            // Handle unhandled exceptions
//            if (context.Exception != null)
//            {
//                context.Result = new ObjectResult(new ApiResponse<object>
//                {
//                    Status = "error",
//                    Data = null,
//                    Message = context.Exception.Message,
//                    StatusCode = StatusCodes.Status500InternalServerError
//                })
//                {
//                    StatusCode = StatusCodes.Status500InternalServerError
//                };
//                context.ExceptionHandled = true;
//                return;
//            }

//            // Handle IActionResult types
//            switch (context.Result)
//            {
//                case ObjectResult objectResult:
//                    context.Result = new ObjectResult(new ApiResponse<object>
//                    {
//                        Status = "success",
//                        Data = objectResult.Value,
//                        Message = null,
//                        StatusCode = objectResult.StatusCode ?? StatusCodes.Status200OK
//                    })
//                    {
//                        StatusCode = objectResult.StatusCode ?? StatusCodes.Status200OK
//                    };
//                    break;

//                case NotFoundResult notFoundResult:
//                    context.Result = new ObjectResult(new ApiResponse<object>
//                    {
//                        Status = "error",
//                        Data = null,
//                        Message = context.HttpContext.Items["ErrorMessage"]?.ToString() ?? "Resource not found.",
//                        StatusCode = StatusCodes.Status404NotFound
//                    })
//                    {
//                        StatusCode = StatusCodes.Status404NotFound
//                    };
//                    break;

//                case BadRequestResult badRequestResult:
//                    context.Result = new ObjectResult(new ApiResponse<object>
//                    {
//                        Status = "error",
//                        Data = null,
//                        Message = "Bad request.",
//                        StatusCode = StatusCodes.Status400BadRequest
//                    })
//                    {
//                        StatusCode = StatusCodes.Status400BadRequest
//                    };
//                    break;

//                case CreatedAtActionResult createdAtActionResult:
//                    context.Result = new ObjectResult(new ApiResponse<object>
//                    {
//                        Status = "success",
//                        Data = createdAtActionResult.Value,
//                        Message = null,
//                        StatusCode = StatusCodes.Status201Created
//                    })
//                    {
//                        StatusCode = StatusCodes.Status201Created
//                    };
//                    break;

//                case NoContentResult noContentResult: // Line 77 in the updated code
//                    context.Result = new ObjectResult(new ApiResponse<object>
//                    {
//                        Status = "success",
//                        Data = null,
//                        Message = null,
//                        StatusCode = StatusCodes.Status204NoContent
//                    })
//                    {
//                        StatusCode = StatusCodes.Status204NoContent
//                    };
//                    break;

//                case StatusCodeResult statusCodeResult:
//                    context.Result = new ObjectResult(new ApiResponse<object>
//                    {
//                        Status = statusCodeResult.StatusCode >= 400 ? "error" : "success",
//                        Data = null,
//                        Message = context.HttpContext.Items["ErrorMessage"]?.ToString(),
//                        StatusCode = statusCodeResult.StatusCode
//                    })
//                    {
//                        StatusCode = statusCodeResult.StatusCode
//                    };
//                    break;

//                default:
//                    // Handle other result types if needed
//                    break;
//            }
//        }
//    }
//}
