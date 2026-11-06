using System.ComponentModel.DataAnnotations;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.DTOs.User;
using PontoDaFarra.Exceptions;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.services.Interface;
using PontoDaFarra.Services;

namespace PontoDaFarra.services;

public class UserServices(
    IUserRepository userRepository,
    ILegalService legalService,
    ITokenService tokenService) : IUserService
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    private static UserAdminResponse ToAdminResponse(User user) =>
        new()
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        };

    public async Task<PagedResponse<UserAdminResponse>> GetAllUsersAsync(UserFilterQuery query)
    {
        int page = Math.Max(query.Page, 1);
        int pageSize = Math.Clamp(query.PageSize, 1, 50);

        var (items, total) = await userRepository.GetAllUsersAsync(query, page, pageSize);

        return new PagedResponse<UserAdminResponse>
        {
            Items = items.Select(ToAdminResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<UserAdminResponse> GetUserByIdAsync(Guid userId)
    {
        var user = await userRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        return ToAdminResponse(user);
    }

    public async Task<CreateResponse> CreateUserAsync(UserCreate dto, string? ipAddress = null)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        ValidatePassword(dto.Password);

        if (await userRepository.EmailExistsAsync(email))
            throw new ConflictException("Email already registered");

        var userId = Guid.NewGuid();
        var consents = await legalService.PrepareRegistrationConsentsAsync(
            userId,
            dto.LegalDocumentIds,
            ipAddress);

        var userCreate = BuildUser(dto, email, userId, consents);
        var refreshToken = tokenService.GenerateRefreshToken();
        var user = await userRepository.CreateUserAsync(userCreate, BuildRefreshToken(refreshToken));
        return BuildCreateResponse(user, refreshToken.Token);
    }

    private static User BuildUser(
        UserCreate dto,
        string email,
        Guid userId,
        List<UserConsent> consents) => new()
        {
            UserId = userId,
            Name = dto.Name.Trim(),
            Email = email,
            Password = PasswordService.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Consents = consents
        };

    private static RefreshToken BuildRefreshToken(GeneratedRefreshToken refreshToken) => new()
    {
        TokenHash = refreshToken.TokenHash,
        ExpiresAt = refreshToken.ExpiresAt
    };

    private CreateResponse BuildCreateResponse(User user, string refreshToken)
    {
        var accessToken = tokenService.GenerateAccessToken(
            user.UserId,
            user.Email,
            user.Name,
            user.PhoneNumber,
            [user.Role]);

        return new CreateResponse
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            RefreshToken = refreshToken,
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
