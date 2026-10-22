using PontoDaFarra.DTOs.Auth;
using PontoDaFarra.Exceptions;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.Services;

namespace PontoDaFarra.services;

public class AuthServices(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService)
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetUserByEmailAsync(email);

        if (user is null || !PasswordService.VerifyPassword(request.Password, user.Password))
            throw new UnauthorizedException("Invalid email or password");

        await refreshTokenRepository.RevokeAllByUserIdAsync(user.UserId);

        var accessToken = tokenService.GenerateAccessToken(user.UserId, user.Email, user.Name, user.PhoneNumber, [user.Role]);
        var refreshToken = tokenService.GenerateRefreshToken();

        await refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = refreshToken.TokenHash,
            ExpiresAt = refreshToken.ExpiresAt
        });


        return new AuthResponse(accessToken, refreshToken.Token);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var hash = tokenService.HashToken(refreshToken);
        var stored = await refreshTokenRepository.GetByHashAsync(hash);

        if (stored is null)
            throw new UnauthorizedException("Invalid or expired refresh token");

        if (stored.IsRevoked && stored.ReplacedByTokenHash is not null)
        {
            await refreshTokenRepository.RevokeAllByUserIdAsync(stored.UserId);
            throw new UnauthorizedException("Invalid or expired refresh token");
        }

        if (!stored.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token");

        var user = await userRepository.GetUserByIdAsync(stored.UserId)
            ?? throw new UnauthorizedException("Invalid or expired refresh token");

        var newRefreshToken = tokenService.GenerateRefreshToken();

        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByTokenHash = newRefreshToken.TokenHash;

        await refreshTokenRepository.RotateAsync(stored, new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = newRefreshToken.TokenHash,
            ExpiresAt = newRefreshToken.ExpiresAt
        });

        var accessToken = tokenService.GenerateAccessToken(user.UserId, user.Email, user.Name, user.PhoneNumber, [user.Role]);

        return new AuthResponse(accessToken, newRefreshToken.Token);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var hash = tokenService.HashToken(refreshToken);
        var stored = await refreshTokenRepository.GetByHashAsync(hash);

        if (stored is { IsActive: true })
        {
            stored.RevokedAt = DateTime.UtcNow;
            await refreshTokenRepository.UpdateAsync(stored);
        }
    }
}