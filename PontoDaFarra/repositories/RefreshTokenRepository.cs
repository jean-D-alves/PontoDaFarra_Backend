using Microsoft.EntityFrameworkCore;
using PontoDaFarra.context;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;

namespace PontoDaFarra.repositories
{
    public class RefreshTokenRepository(AppDbContext _context) : IRefreshTokenRepository
    {
        public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public async Task AddAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task RotateAsync(RefreshToken oldToken, RefreshToken newToken)
        {
            _context.RefreshTokens.Update(oldToken);
            _context.RefreshTokens.Add(newToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllByUserIdAsync(Guid userId)
        {
            var active = await _context.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ToListAsync();

            foreach (var token in active)
                token.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteExpiredAsync(DateTime cutoff)
        {
            return await _context.RefreshTokens
                .Where(t => t.ExpiresAt < cutoff)
                .ExecuteDeleteAsync();
        }
    }
}