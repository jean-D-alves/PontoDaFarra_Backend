using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PontoDaFarra.Exceptions;

namespace PontoDaFarra.Middleware;

public sealed class GlobalExceptionHandler(
    ProblemDetailsWriter problemDetailsWriter,
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Request aborted by the client: {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            return true;
        }

        var (statusCode, title, detail) = Map(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning("{StatusCode} {Method} {Path}: {Message}", statusCode, httpContext.Request.Method, httpContext.Request.Path, detail);
        }

        await problemDetailsWriter.WriteAsync(httpContext, statusCode, title, detail, cancellationToken);

        return true;
    }

    private (int StatusCode, string Title, string Detail) Map(Exception exception) => exception switch
    {
        AppException appException =>
            (appException.StatusCode, appException.Title, appException.Message),

        BadHttpRequestException badRequest =>
            (StatusCodes.Status400BadRequest, ProblemDetailsWriter.Title(StatusCodes.Status400BadRequest), badRequest.Message),

        JsonException json =>
            (StatusCodes.Status400BadRequest, ProblemDetailsWriter.Title(StatusCodes.Status400BadRequest), json.Message),

        FormatException format =>
            (StatusCodes.Status400BadRequest, ProblemDetailsWriter.Title(StatusCodes.Status400BadRequest), format.Message),

        ArgumentException argument =>
            (StatusCodes.Status400BadRequest, ProblemDetailsWriter.Title(StatusCodes.Status400BadRequest), argument.Message),

        KeyNotFoundException notFound =>
            (StatusCodes.Status404NotFound, ProblemDetailsWriter.Title(StatusCodes.Status404NotFound), notFound.Message),

        UnauthorizedAccessException unauthorized =>
            (StatusCodes.Status401Unauthorized, ProblemDetailsWriter.Title(StatusCodes.Status401Unauthorized), unauthorized.Message),

        DbUpdateException dbUpdate =>
            MapDbUpdateException(dbUpdate),

        TimeoutException timeout =>
            (StatusCodes.Status504GatewayTimeout, ProblemDetailsWriter.Title(StatusCodes.Status504GatewayTimeout), timeout.Message),

        _ =>
            MapUnexpected(exception)
    };

    private (int StatusCode, string Title, string Detail) MapUnexpected(Exception exception)
    {
        var npgsql = FindInChain<NpgsqlException>(exception);

        if (npgsql is not null)
        {
            return (StatusCodes.Status503ServiceUnavailable,
                ProblemDetailsWriter.Title(StatusCodes.Status503ServiceUnavailable),
                UnexpectedDetail(npgsql, StatusCodes.Status503ServiceUnavailable));
        }

        var timeout = FindInChain<TimeoutException>(exception);

        if (timeout is not null)
        {
            return (StatusCodes.Status504GatewayTimeout,
                ProblemDetailsWriter.Title(StatusCodes.Status504GatewayTimeout),
                timeout.Message);
        }

        return (StatusCodes.Status500InternalServerError,
            ProblemDetailsWriter.Title(StatusCodes.Status500InternalServerError),
            UnexpectedDetail(exception));
    }

    private (int StatusCode, string Title, string Detail) MapDbUpdateException(DbUpdateException exception)
    {
        var postgresException = FindPostgresException(exception);

        if (postgresException is null)
            return MapUnexpected(exception);

        if (IsConnectionError(postgresException))
            return (StatusCodes.Status503ServiceUnavailable, ProblemDetailsWriter.Title(StatusCodes.Status503ServiceUnavailable), UnexpectedDetail(postgresException, StatusCodes.Status503ServiceUnavailable));

        return postgresException.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation =>
                (StatusCodes.Status409Conflict, ProblemDetailsWriter.Title(StatusCodes.Status409Conflict), "A resource with the same unique value already exists"),
            PostgresErrorCodes.ForeignKeyViolation =>
                (StatusCodes.Status409Conflict, ProblemDetailsWriter.Title(StatusCodes.Status409Conflict), "The request conflicts with a related resource"),
            _ => (StatusCodes.Status500InternalServerError, ProblemDetailsWriter.Title(StatusCodes.Status500InternalServerError), UnexpectedDetail(exception))
        };
    }

    private static PostgresException? FindPostgresException(Exception exception) =>
        FindInChain<PostgresException>(exception);

    private static TException? FindInChain<TException>(Exception exception) where TException : Exception
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is TException match)
                return match;
        }

        return null;
    }

    private static bool IsConnectionError(PostgresException exception) =>
        exception.SqlState.StartsWith("08", StringComparison.Ordinal);

    private string UnexpectedDetail(Exception exception, int statusCode = StatusCodes.Status500InternalServerError) =>
        environment.IsDevelopment() ? exception.Message : ProblemDetailsWriter.Detail(statusCode);
}
