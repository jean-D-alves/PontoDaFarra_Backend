using System.ComponentModel.DataAnnotations;
using PontoDaFarra.DTOs.User;
using PontoDaFarra.Exceptions;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.services.Interface;
using PontoDaFarra.Services;

namespace PontoDaFarra.services;

public class UserServices(
    IUserRepository userRepository,
    ITokenService tokenService) : IUserService
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public async Task<CreateResponse> CreateUserAsync(UserCreate dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        ValidatePassword(dto.Password);

        if (await userRepository.EmailExistsAsync(email))
            throw new ConflictException("Email already registered");

        var userCreate = new User
        {
            Name = dto.Name.Trim(),
            Email = email,
            Password = PasswordService.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber.Trim()
        };

        var refreshToken = tokenService.GenerateRefreshToken();

        var user = await userRepository.CreateUserAsync(userCreate, new RefreshToken
        {
            TokenHash = refreshToken.TokenHash,
            ExpiresAt = refreshToken.ExpiresAt
        });

        var accessToken = tokenService.GenerateAccessToken(user.UserId, user.Email, user.Name, user.PhoneNumber, [user.Role]);

        return new CreateResponse
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            RefreshToken = refreshToken.Token,
            AccessToken = accessToken
        };
    }

    public async Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUser dto)
    {
        var user = await userRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            user.Name = dto.Name.Trim();

        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            user.PhoneNumber = dto.PhoneNumber.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (!EmailValidator.IsValid(email))
                throw new BadRequestException("Invalid email format");

            if (email != user.Email)
            {
                if (await userRepository.EmailExistsAsync(email))
                    throw new ConflictException("Email already registered");
                user.Email = email;
            }
        }

        await userRepository.UpdateUserAsync(user);

        return new UserResponse
        {
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        };
    }

    public async Task<UserResponse> UpdateUserRoleAsync(Guid adminId, Guid userId)
    {
        if (adminId == Guid.Empty || userId == Guid.Empty)
            throw new BadRequestException("User ID cannot be null");
        if (adminId == userId)
            throw new BadRequestException("You cannot change your own role");
        var user = await userRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException("User not found");
        var role = user.Role == "Admin" ? "Client" : "Admin";
        user.Role = role.Trim();

        await userRepository.UpdateUserAsync(user);

        return new UserResponse
        {
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        };
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await userRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        await userRepository.DeleteUserAsync(user);
    }

    private static void ValidatePassword(string password)
    {
        if (password.Length < 8)
            throw new BadRequestException("Password must be at least 8 characters long");

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new BadRequestException("Password must contain at least one letter and one number");
    }
}
