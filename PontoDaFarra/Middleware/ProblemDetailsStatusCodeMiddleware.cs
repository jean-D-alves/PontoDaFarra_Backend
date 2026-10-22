namespace PontoDaFarra.Middleware;

public sealed class ProblemDetailsStatusCodeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, ProblemDetailsWriter problemDetailsWriter)
    {
        httpContext.Response.OnStarting(() => WriteProblemDetailsIfBodyIsEmptyAsync(httpContext, problemDetailsWriter));

        await next(httpContext);
    }

    public static async Task WriteProblemDetailsIfBodyIsEmptyAsync(HttpContext httpContext, ProblemDetailsWriter problemDetailsWriter)
    {
        var response = httpContext.Response;

        if (response.HasStarted)
            return;

        var statusCode = response.StatusCode;

        if (statusCode < StatusCodes.Status400BadRequest || statusCode >= 600)
            return;

        if (!string.IsNullOrEmpty(response.ContentType))
            return;

        await problemDetailsWriter.WriteAsync(
            httpContext,
            statusCode,
            ProblemDetailsWriter.Title(statusCode),
            ProblemDetailsWriter.Detail(statusCode));
    }
}
