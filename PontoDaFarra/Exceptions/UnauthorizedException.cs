namespace PontoDaFarra.Exceptions;

public class UnauthorizedException(string message)
    : AppException(message, StatusCodes.Status401Unauthorized, "Unauthorized");
