namespace PontoDaFarra.Exceptions;

public abstract class AppException(string message, int statusCode, string title) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public string Title { get; } = title;
}
