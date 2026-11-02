using PontoDaFarra.DTOs.User;
using PontoDaFarra.models;

namespace PontoDaFarra.repositories.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<(List<User> Items, int Total)> GetAllUsersAsync(UserFilterQuery filter, int page, int pageSize);
        Task<User> CreateUserAsync(User user, RefreshToken refreshToken);
        Task<User> UpdateUserAsync(User user);
        Task<User> DeleteUserAsync(User user);
    }
}
