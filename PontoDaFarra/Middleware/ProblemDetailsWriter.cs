using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace PontoDaFarra.Middleware;

public sealed class ProblemDetailsWriter
{
    public const string ProblemJsonContentType = "application/problem+json";

    public async Task WriteAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        CancellationToken cancellationToken = default)
    {
        var problemDetails = Create(httpContext, statusCode, title, detail);

        httpContext.Response.StatusCode = statusCode;

        var problemDetailsService = httpContext.RequestServices?.GetService<IProblemDetailsService>();

        if (problemDetailsService is not null)
        {
            var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });

            if (written)
                return;
        }

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: ProblemJsonContentType,
            cancellationToken: cancellationToken);
    }

    public static ProblemDetails Create(HttpContext httpContext, int statusCode, string title, string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Type = ProblemType(statusCode)
        };

        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return problemDetails;
    }

    public static string Title(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Resource not found",
        StatusCodes.Status405MethodNotAllowed => "Method not allowed",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status415UnsupportedMediaType => "Unsupported media type",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable entity",
        StatusCodes.Status429TooManyRequests => "Too many requests",
        StatusCodes.Status500InternalServerError => "Internal Server Error",
        StatusCodes.Status503ServiceUnavailable => "Service unavailable",
        StatusCodes.Status504GatewayTimeout => "Gateway timeout",
        _ => "Error"
    };

    public static string Detail(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "The request could not be understood by the server.",
        StatusCodes.Status401Unauthorized => "A valid access token is required to access this resource.",
        StatusCodes.Status403Forbidden => "You do not have permission to access this resource.",
        StatusCodes.Status404NotFound => "The requested resource was not found.",
        StatusCodes.Status405MethodNotAllowed => "The HTTP method is not supported for this resource.",
        StatusCodes.Status409Conflict => "The request conflicts with the current state of the resource.",
        StatusCodes.Status415UnsupportedMediaType => "The request media type is not supported by this resource.",
        StatusCodes.Status422UnprocessableEntity => "The request was well formed but could not be processed.",
        StatusCodes.Status429TooManyRequests => "Too many requests. Please try again later.",
        StatusCodes.Status500InternalServerError => "An unexpected error occurred.",
        StatusCodes.Status503ServiceUnavailable => "The service is temporarily unavailable.",
        StatusCodes.Status504GatewayTimeout => "The request timed out.",
        _ => "An unexpected error occurred."
    };

    public static string ProblemType(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1",
        StatusCodes.Status401Unauthorized => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.2",
        StatusCodes.Status403Forbidden => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.4",
        StatusCodes.Status404NotFound => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.5",
        StatusCodes.Status405MethodNotAllowed => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.6",
        StatusCodes.Status409Conflict => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.10",
        StatusCodes.Status415UnsupportedMediaType => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.16",
        StatusCodes.Status422UnprocessableEntity => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.21",
        StatusCodes.Status429TooManyRequests => "https://www.rfc-editor.org/rfc/rfc6585#section-4",
        StatusCodes.Status500InternalServerError => "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.1",
        StatusCodes.Status503ServiceUnavailable => "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.4",
        StatusCodes.Status504GatewayTimeout => "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.5",
        _ => "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1"
    };
}
