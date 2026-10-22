namespace PontoDaFarra.Exceptions;

public class ForbiddenException(string message)
    : AppException(message, StatusCodes.Status403Forbidden, "Forbidden");
