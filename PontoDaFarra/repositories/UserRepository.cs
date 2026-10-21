using Microsoft.EntityFrameworkCore;
using PontoDaFarra.context;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;

namespace PontoDaFarra.repositories
{
    public class UserRepository(AppDbContext _context) : IUserRepository
    {

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            User? user = await _context.Users.FindAsync(userId);
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user;
        }

        public async Task<User> CreateUserAsync(User user, RefreshToken refreshToken)
        {
            _context.Users.Add(user);
            refreshToken.UserId = user.UserId;
            _context.RefreshTokens.Add(refreshToken);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Email already registered");
            }

            return user;
        }
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}
