using PontoDaFarra.models;

namespace PontoDaFarra.repositories.Interface
{
    public interface IUserConsentRepository
    {
        Task<List<Guid>> GetAcceptedDocumentIdsAsync(Guid userId);
        Task<List<UserConsent>> GetByUserIdAsync(Guid userId);
        Task<bool> HasAnyForDocumentAsync(Guid legalDocumentId);
        Task AddRangeAsync(IEnumerable<UserConsent> consents);
    }
}
