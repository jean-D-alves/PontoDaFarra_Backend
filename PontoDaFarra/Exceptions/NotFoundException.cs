namespace PontoDaFarra.Exceptions;

public class NotFoundException(string message)
    : AppException(message, StatusCodes.Status404NotFound, "Resource not found");
