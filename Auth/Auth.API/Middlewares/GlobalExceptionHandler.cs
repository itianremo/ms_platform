using System.Net;
using System.Text.Json;
using global::Auth.Domain.Exceptions;

namespace Auth.API.Middlewares;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ErrorResponse
        {
            Message = exception.Message
        };

        switch (exception)
        {
            case RequiresVerificationException verificationEx:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Error = "RequiresVerification";
                errorResponse.Status = verificationEx.RequiredStatus.ToString();
                errorResponse.Phone = verificationEx.Phone;
                break;

            case RequiresAdminApprovalException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Error = "RequiresAdminApproval";
                break;
                
            case AccountBannedException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Error = "AccountBanned";
                break;

            case UserSoftDeletedException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Error = "AccountSoftDeleted";
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Error = "Unauthorized";
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Error = "NotFound";
                break;

            default:
                _logger.LogError(exception, "An unhandled exception occurred.");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Error = "InternalServerError";
                errorResponse.Message = "An unexpected error occurred.";
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public string? Error { get; set; }
    public string? Message { get; set; }
    public string? Status { get; set; } // For Verification Status
    public string? Phone { get; set; } // For Verification Phone
}
