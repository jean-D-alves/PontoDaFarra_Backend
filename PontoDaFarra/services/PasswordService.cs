using BCrypt.Net;
using PontoDaFarra.Exceptions;

namespace PontoDaFarra.services
{

    public class PasswordService
    {

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        public static void ValidatePassword(string password)
        {
            if (password.Length < 8)
                throw new BadRequestException("Password must be at least 8 characters long");

            if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
                throw new BadRequestException("Password must contain at least one letter and one number");
        }
    }
}