using System.Security.Claims;

namespace PontoDaFarra.Services;

public record GeneratedRefreshToken(string Token, string TokenHash, DateTime ExpiresAt);

public interface ITokenService
{
    /// <summary>Generates a short-lived signed JWT access token.</summary>
    string GenerateAccessToken(Guid userId, string email, string name, IEnumerable<string> roles);

    /// <summary>
    /// Generates an opaque random refresh token. Send <c>Token</c> to the client
    /// and store only <c>TokenHash</c> in the database.
    /// </summary>
    GeneratedRefreshToken GenerateRefreshToken();

    /// <summary>
    /// Validates an access token. Returns null if it is invalid.
    /// Use validateLifetime: false in the refresh flow to read claims from an expired token.
    /// </summary>
    ClaimsPrincipal? ValidateAccessToken(string accessToken, bool validateLifetime = true);

    /// <summary>SHA-256 hash (Base64) used to store/look up refresh tokens.</summary>
    string HashToken(string token);
}