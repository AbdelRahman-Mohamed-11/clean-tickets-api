using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace TicketingSystem.API.Middlewares
{
    public class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment env) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid().ToString();

            logger.LogError(exception,
                "Unhandled exception occurred. CorrelationId: {CorrelationId}",
                correlationId);

            var problemDetails = new ProblemDetails
            {
                Title = "An unexpected error occurred",
                Status = (int)HttpStatusCode.InternalServerError,
                Type = "https://httpstatuses.com/500",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["correlationId"] = correlationId;
            problemDetails.Extensions["code"] = "UNEXPECTED_ERROR";

            if (env.IsDevelopment())
            {
                problemDetails.Detail = GetFullExceptionMessage(exception);
                problemDetails.Extensions["exception"] = GetExceptionDetails(exception);
            }
            else
            {
                problemDetails.Detail = "An error occurred while processing your request. Please try again later.";
            }

            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private string GetFullExceptionMessage(Exception ex)
        {
            var sb = new StringBuilder();
            var current = ex;
            int level = 0;

            while (current != null)
            {
                sb.AppendLine($"[Level {level}] {current.Message}");
                current = current.InnerException;
                level++;
            }

            return sb.ToString();
        }

        private object GetExceptionDetails(Exception ex)
        {
            return new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace?.Split('\n'),
                source = ex.Source,
                innerException = ex.InnerException != null ? GetExceptionDetails(ex.InnerException) : null
            };
        }
    }
}