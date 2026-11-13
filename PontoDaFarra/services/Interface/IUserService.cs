using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.DTOs.Auth;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.DTOs.User;
using PontoDaFarra.models;

namespace PontoDaFarra.services.Interface
{
    public interface IUserService
    {
        Task<CreateResponse> CreateUserAsync(UserCreate dto, string? ipAddress = null);
        Task<PagedResponse<UserAdminResponse>> GetAllUsersAsync(UserFilterQuery query);
        Task<UserAdminResponse> GetUserByIdAsync(Guid userId);
        Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUser dto);
        Task<UserResponse> UpdateUserRoleAsync(Guid adminId, Guid userId);
        Task<AuthResponse> ChangeOwnPasswordAsync(Guid userId, ChangePasswordRequest dto);
        Task AdminResetPasswordAsync(Guid adminId, Guid targetUserId, AdminChangePasswordRequest dto);
        Task DeleteUserAsync(Guid userId);
    }
}