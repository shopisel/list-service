using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ListService.Middleware;

public sealed class ApiExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionHandlingMiddleware> _logger;

    public ApiExceptionHandlingMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlingMiddleware> logger)
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (BadHttpRequestException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "The request body is invalid.",
                ex,
                LogLevel.Warning);
        }
        catch (ArgumentException ex)
        {
            await WriteValidationProblemAsync(context, ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "A concurrent update was detected.",
                ex,
                LogLevel.Warning);
        }
        catch (DbUpdateException ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "The database is temporarily unavailable.",
                ex,
                LogLevel.Error);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                ex,
                LogLevel.Error);
        }
    }

    private async Task WriteValidationProblemAsync(HttpContext context, ArgumentException exception)
    {
        if (context.Response.HasStarted)
        {
            throw exception;
        }

        _logger.LogWarning(exception, "Validation failed for {Method} {Path}", context.Request.Method, context.Request.Path);

        var fieldName = string.IsNullOrWhiteSpace(exception.ParamName) ? "error" : exception.ParamName;
        await Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [fieldName] = [exception.Message]
                })
            .ExecuteAsync(context);
    }

    private async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        Exception exception,
        LogLevel logLevel)
    {
        if (context.Response.HasStarted)
        {
            throw exception;
        }

        _logger.Log(
            logLevel,
            exception,
            "Request {Method} {Path} failed with {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            statusCode);

        await Results.Problem(
                statusCode: statusCode,
                title: title)
            .ExecuteAsync(context);
    }
}
