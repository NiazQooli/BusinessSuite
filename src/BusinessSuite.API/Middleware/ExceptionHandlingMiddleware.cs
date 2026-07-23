using System.Net;
using BusinessSuite.Application.Common.Exceptions;
using BusinessSuite.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BusinessSuite.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (
                (int)HttpStatusCode.BadRequest,
                "Validation Error",
                validationException.Errors),
            NotFoundException notFoundException => (
                (int)HttpStatusCode.NotFound,
                "Not Found",
                new Dictionary<string, string[]> { ["error"] = new[] { notFoundException.Message } }),
            DomainException domainException => (
                (int)HttpStatusCode.BadRequest,
                "Domain Error",
                new Dictionary<string, string[]> { ["error"] = new[] { domainException.Message } }),
            UnauthorizedException unauthorizedException => (
                (int)HttpStatusCode.Unauthorized,
                "Unauthorized",
                new Dictionary<string, string[]> { ["error"] = new[] { unauthorizedException.Message } }),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "Server Error",
                new Dictionary<string, string[]>
                {
                    ["error"] = new[]
                    {
                        _environment.IsDevelopment()
                            ? exception.Message
                            : "An unexpected error occurred."
                    }
                })
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Title}", title);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new ValidationProblemDetails(errors)
        {
            Status = statusCode,
            Title = title,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
