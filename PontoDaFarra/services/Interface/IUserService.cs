using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.DTOs.User;
using PontoDaFarra.models;

namespace PontoDaFarra.services.Interface
{
    public interface IUserService
    {
        Task<CreateResponse> CreateUserAsync(UserCreate dto);
        Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUser dto);
        Task<UserResponse> UpdateUserRoleAsync(Guid adminId, Guid userId);
        Task DeleteUserAsync(Guid userId);
    }
}