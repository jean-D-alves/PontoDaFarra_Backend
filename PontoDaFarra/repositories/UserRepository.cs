using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Npgsql;
using PontoDaFarra.context;
using PontoDaFarra.DTOs.User;
using PontoDaFarra.Exceptions;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;

namespace PontoDaFarra.repositories
{
    public class UserRepository(AppDbContext _context) : IUserRepository
    {
        private static string EscapeLike(string value) =>
            value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

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

        public async Task<(List<User> Items, int Total)> GetAllUsersAsync(
            UserFilterQuery filter, int page, int pageSize)
        {
            IQueryable<User> query = _context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                string role = filter.Role.Trim();
                query = query.Where(u => EF.Functions.ILike(u.Role, role));
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string pattern = $"%{EscapeLike(filter.Search.Trim())}%";

                query = query.Where(u =>
                    EF.Functions.ILike(u.Name, pattern)
                    || EF.Functions.ILike(u.Email, pattern));
            }

            int total = await query.CountAsync();

            List<User> items = await query
                .OrderBy(u => u.Name)
                .ThenBy(u => u.UserId) // desempate para a paginação ser estável
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
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
            List<Order> orders = await _context.Orders
                .Where(o => o.UserId == user.UserId)
                .ToListAsync();

            foreach (Order order in orders)
            {
                order.CustomerName = "Deleted user";
                order.CustomerPhone = null;
            }

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
