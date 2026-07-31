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
        catch (Exception ex) when (IsClientAbort(ex, context))
        {
            // Find/search often aborts prior HTTP calls when filters change — not a server fault.
            _logger.LogDebug(ex, "Request aborted by client: {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static bool IsClientAbort(Exception exception, HttpContext context)
    {
        if (!context.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        return exception is OperationCanceledException or TaskCanceledException
            || exception.GetBaseException() is OperationCanceledException or TaskCanceledException;
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // If response already started / client gone, skip writing problem details.
        if (context.RequestAborted.IsCancellationRequested || context.Response.HasStarted)
        {
            return;
        }

        var (statusCode, title, errors) = exception switch
        {
            OperationCanceledException or TaskCanceledException => (
                499,
                "Request Canceled",
                new Dictionary<string, string[]> { ["error"] = new[] { "The request was canceled." } }),
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
        else if (statusCode != 499)
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
