using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Npgsql;
using PontoDaFarra.context;
using PontoDaFarra.Exceptions;
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
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new ConflictException("Email already registered");
            }

            return user;
        }
        public async Task<User> UpdateUserAsync(User user)
        {
            EntityEntry<User> entry = _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<User> DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}
