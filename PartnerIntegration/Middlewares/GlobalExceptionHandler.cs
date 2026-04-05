using Microsoft.AspNetCore.Diagnostics;
using PartnerBFF.Application.Exceptions;
using PartnerBFF.Application.Models;

namespace PartnerBFF.API.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception,
                "Exception occurred: {Message} | TraceId: {TraceId}",
                exception.Message,
                httpContext.TraceIdentifier);

            var errorResponse = exception switch
            {
                TransactionValidationException ex => new ErrorResponse
                {
                    TraceId = httpContext.TraceIdentifier,
                    StatusCode = ex.StatusCode,
                    Message = ex.Message,
                    Errors = ex.Errors
                },

                PartnerVerificationException ex => new ErrorResponse
                {
                    TraceId = httpContext.TraceIdentifier,
                    StatusCode = ex.StatusCode,
                    Message = ex.Message
                },

                MessagePublishException ex => new ErrorResponse
                {
                    TraceId = httpContext.TraceIdentifier,
                    StatusCode = ex.StatusCode,
                    Message = ex.Message
                },

                // Catch all — never expose internal details
                _ => new ErrorResponse
                {
                    TraceId = httpContext.TraceIdentifier,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred"
                }
            };

            
            httpContext.Response.StatusCode = errorResponse.StatusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true; // true = exception is handled, stop propagation
        }
    }
}
