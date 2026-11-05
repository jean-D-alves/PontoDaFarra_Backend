using Microsoft.EntityFrameworkCore;
using PontoDaFarra.context;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;

namespace PontoDaFarra.repositories
{
    public class UserConsentRepository(AppDbContext context) : IUserConsentRepository
    {
        public async Task<List<Guid>> GetAcceptedDocumentIdsAsync(Guid userId)
        {
            return await context.UserConsents
                .AsNoTracking()
                .Where(consent => consent.UserId == userId)
                .Select(consent => consent.LegalDocumentId)
                .ToListAsync();
        }

        public async Task<List<UserConsent>> GetByUserIdAsync(Guid userId)
        {
            return await context.UserConsents
                .AsNoTracking()
                .Include(consent => consent.LegalDocument)
                .Where(consent => consent.UserId == userId)
                .OrderByDescending(consent => consent.AcceptedAt)
                .ToListAsync();
        }

        public async Task<bool> HasAnyForDocumentAsync(Guid legalDocumentId)
        {
            return await context.UserConsents
                .AsNoTracking()
                .AnyAsync(consent => consent.LegalDocumentId == legalDocumentId);
        }

        public async Task AddRangeAsync(IEnumerable<UserConsent> consents)
        {
            context.UserConsents.AddRange(consents);
            await context.SaveChangesAsync();
        }
    }
}
