using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.DTOs.User;
using PontoDaFarra.models;

namespace PontoDaFarra.services.Interface
{
    public interface IUserService
    {
        Task<CreateResponse> CreateUserAsync(UserCreate dto);
        Task<PagedResponse<UserAdminResponse>> GetAllUsersAsync(UserFilterQuery query);
        Task<UserAdminResponse> GetUserByIdAsync(Guid userId);
        Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUser dto);
        Task<UserResponse> UpdateUserRoleAsync(Guid adminId, Guid userId);
        Task DeleteUserAsync(Guid userId);
    }
}