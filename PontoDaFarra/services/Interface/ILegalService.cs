using PontoDaFarra.DTOs.Legal;
using PontoDaFarra.models;

namespace PontoDaFarra.services.Interface
{
    public interface ILegalService
    {
        Task<LegalDocumentResponseDto> GetActiveByTypeAsync(string type);
        Task<List<LegalDocumentResponseDto>> GetPendingAsync(Guid userId);
        Task AcceptAsync(Guid userId, AcceptLegalDocumentsDto dto, string? ipAddress);
        Task<List<UserConsentResponseDto>> GetConsentsAsync(Guid userId);
        Task<List<LegalDocumentResponseDto>> GetAllDocumentsAsync();
        Task<LegalDocumentResponseDto> PublishAsync(PublishLegalDocumentDto dto);
        Task<LegalDocumentResponseDto> UpdateAsync(Guid legalDocumentId, UpdateLegalDocumentDto dto);
        Task<List<UserConsent>> PrepareRegistrationConsentsAsync(
            Guid userId,
            IReadOnlyCollection<Guid> legalDocumentIds,
            string? ipAddress);
    }
}
