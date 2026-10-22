using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PontoDaFarra.Services;

namespace PontoDaFarra.services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _signingKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;

    public TokenService(IConfiguration config)
    {
        var key = config["JWT_KEY"]
                  ?? throw new InvalidOperationException("JWT_KEY not configured");

        if (Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("JWT_KEY must be at least 32 bytes long");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        _issuer = config["ISSUER"] ?? throw new InvalidOperationException("ISSUER not configured");
        _audience = config["AUDIENCE"] ?? throw new InvalidOperationException("AUDIENCE not configured");
        _accessTokenMinutes = config.GetValue("EXPIRES_MINUTES", 15);
        _refreshTokenDays = config.GetValue("REFRESH_EXPIRES_DAYS", 7);
    }

    public string GenerateAccessToken(Guid userId, string email, string name, string? phoneNumber, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.Add(new Claim("phone_number", phoneNumber ?? string.Empty));
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public GeneratedRefreshToken GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Base64UrlEncoder.Encode(bytes);

        return new GeneratedRefreshToken(
            Token: token,
            TokenHash: HashToken(token),
            ExpiresAt: DateTime.UtcNow.AddDays(_refreshTokenDays));
    }

    public ClaimsPrincipal? ValidateAccessToken(string accessToken, bool validateLifetime = true)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(accessToken, parameters, out var validated);

            // Reject tokens signed with any algorithm other than HS256
            if (validated is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return null;
        }
    }

    public string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
}