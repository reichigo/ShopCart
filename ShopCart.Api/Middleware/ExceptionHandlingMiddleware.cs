using System.Net;
using System.Text.Json;
using ShopCart.CrossCutting;

namespace ShopCart.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        _logger.LogError(exception, "An unhandled exception has occurred");

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";
        
        if (exception is GlobalException globalException)
        {
            statusCode = globalException.StatusCode;
            message = globalException.Message;
        }
        else if (exception is ArgumentException || exception is ArgumentNullException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = exception.Message;
        }
        else
        {
            if (context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
            {
                message = exception.Message;
            }
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message = message,
            // Incluir detalhes do erro apenas em ambiente de desenvolvimento
            details = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true
                ? new
                {
                    exception = exception.GetType().Name,
                    stackTrace = exception.StackTrace,
                    innerException = exception.InnerException?.Message
                }
                : null
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}

// Extensão para facilitar o registro do middleware
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}