using PontoDaFarra.models;

namespace PontoDaFarra.repositories.Interface
{
    public interface ILegalDocumentRepository
    {
        Task<LegalDocument?> GetActiveByTypeAsync(LegalDocumentType type);
        Task<List<LegalDocument>> GetActiveAsync();
        Task<List<LegalDocument>> GetAllAsync();
        Task<LegalDocument?> GetByIdAsync(Guid legalDocumentId);
        Task<List<LegalDocument>> GetByIdsAsync(IReadOnlyCollection<Guid> legalDocumentIds);
        Task<bool> VersionExistsAsync(
            LegalDocumentType type,
            string version,
            Guid? excludeLegalDocumentId = null);
        Task<LegalDocument> PublishAsync(LegalDocument document);
        Task<LegalDocument> UpdateAsync(LegalDocument document);
    }
}
