using Serilog;
using System.Net;
using System.Text.Json;
using WeatherInfo.API.Exceptions;

namespace WeatherInfo.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
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
            HttpStatusCode status;
            string message;
            object? details = null;

            switch (exception)
            {
                case CityNotFoundException:
                    status = HttpStatusCode.NotFound;
                    message = exception.Message;
                    details = exception.Data["Details"];
                    break;
                case InvalidDateFormatException:
                    status = HttpStatusCode.BadRequest;
                    message= exception.Message;
                    details = exception.Data["Details"];
                    break;
                case UpstreamBadRequestException:
                    status = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    details = exception.Data["Details"];
                    break;
                case UpstreamUnavailableException:
                    status = HttpStatusCode.BadGateway;
                    message = exception.Message;
                    details = exception.Data["Details"];
                    break;
                default:
                    status = HttpStatusCode.InternalServerError;
                    message = "An unexpected error occurred";
                    break;
            }

            Log.Error(exception, "HTTP {StatusCode} - {Message}", (int)status, message);

            var errorResponse = new
            {
                error = message,
                status = (int)status,
                details = details ??(_env.IsDevelopment() ? exception.StackTrace :null),
                timestamp = DateTime.UtcNow
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            await context.Response.WriteAsJsonAsync(errorResponse, new JsonSerializerOptions
            {
                WriteIndented = true,
            });
        }
    }
}
