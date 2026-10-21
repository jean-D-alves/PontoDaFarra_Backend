using PontoDaFarra.models;

namespace PontoDaFarra.repositories.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user, RefreshToken refreshToken);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
    }
}
