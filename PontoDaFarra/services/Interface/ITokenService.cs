using System.Security.Claims;

namespace PontoDaFarra.Services;

public record GeneratedRefreshToken(string Token, string TokenHash, DateTime ExpiresAt);

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, string name, string? phoneNumber, IEnumerable<string> roles);


    GeneratedRefreshToken GenerateRefreshToken();


    ClaimsPrincipal? ValidateAccessToken(string accessToken, bool validateLifetime = true);

    string HashToken(string token);
}