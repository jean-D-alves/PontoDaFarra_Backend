using PontoDaFarra.models;

namespace PontoDaFarra.repositories.Interface
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByHashAsync(string tokenHash);
        Task AddAsync(RefreshToken token);
        Task UpdateAsync(RefreshToken token);

        Task RotateAsync(RefreshToken oldToken, RefreshToken newToken);

        Task RevokeAllByUserIdAsync(Guid userId);

        Task<int> DeleteExpiredAsync(DateTime cutoff);
    }
}