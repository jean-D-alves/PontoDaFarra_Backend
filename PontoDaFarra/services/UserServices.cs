using PontoDaFarra.DTOs.User;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.Services;

namespace PontoDaFarra.services;

public class UserServices(
    IUserRepository userRepository,
    ITokenService tokenService)
{
    public async Task<CreateResponse> CreateUserAsync(UserCreate dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await userRepository.EmailExistsAsync(email))
            throw new InvalidOperationException("Email already registered");

        var userCreate = new User
        {
            Name = dto.Name.Trim(),
            Email = email,
            Password = PasswordService.HashPassword(dto.Password)
        };

        var refreshToken = tokenService.GenerateRefreshToken();

        var user = await userRepository.CreateUserAsync(userCreate, new RefreshToken
        {
            TokenHash = refreshToken.TokenHash,
            ExpiresAt = refreshToken.ExpiresAt
        });

        var accessToken = tokenService.GenerateAccessToken(user.UserId, user.Email, user.Name, [user.Role]);

        return new CreateResponse
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            RefreshToken = refreshToken.Token,
            AccessToken = accessToken
        };
    }
}
