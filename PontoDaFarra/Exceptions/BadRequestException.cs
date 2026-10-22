namespace PontoDaFarra.Exceptions;

public class BadRequestException(string message)
    : AppException(message, StatusCodes.Status400BadRequest, "Bad request");
