using PontoDaFarra.DTOs.Legal;
using PontoDaFarra.Exceptions;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.services
{
    public class LegalService(
        ILegalDocumentRepository legalDocumentRepository,
        IUserConsentRepository userConsentRepository,
        ILogger<LegalService> logger) : ILegalService
    {
        public async Task<LegalDocumentResponseDto> GetActiveByTypeAsync(string type)
        {
            var documentType = ParseType(type);
            var document = await legalDocumentRepository.GetActiveByTypeAsync(documentType)
                ?? throw new NotFoundException("No active legal document found for this type");

            return ToResponse(document);
        }

        public async Task<List<LegalDocumentResponseDto>> GetPendingAsync(Guid userId)
        {
            var activeDocuments = await legalDocumentRepository.GetActiveAsync();
            var acceptedIds = await userConsentRepository.GetAcceptedDocumentIdsAsync(userId);

            return activeDocuments
                .Where(document => !acceptedIds.Contains(document.LegalDocumentId))
                .Select(ToResponse)
                .ToList();
        }

        public async Task AcceptAsync(Guid userId, AcceptLegalDocumentsDto dto, string? ipAddress)
        {
            EnsureValidIds(dto.LegalDocumentIds);
            var documents = await GetActiveDocumentsAsync(dto.LegalDocumentIds);
            var acceptedIds = await userConsentRepository.GetAcceptedDocumentIdsAsync(userId);
            var consents = documents
                .Where(document => !acceptedIds.Contains(document.LegalDocumentId))
                .Select(document => CreateConsent(userId, document.LegalDocumentId, ipAddress))
                .ToList();

            if (consents.Count == 0)
                return;

            await userConsentRepository.AddRangeAsync(consents);
            logger.LogInformation("User {UserId} accepted {ConsentCount} legal document(s)", userId, consents.Count);
        }

        public async Task<List<UserConsentResponseDto>> GetConsentsAsync(Guid userId)
        {
            var consents = await userConsentRepository.GetByUserIdAsync(userId);
            return consents.Select(ToConsentResponse).ToList();
        }

        public async Task<List<LegalDocumentResponseDto>> GetAllDocumentsAsync()
        {
            var documents = await legalDocumentRepository.GetAllAsync();
            return documents.Select(ToResponse).ToList();
        }

        public async Task<LegalDocumentResponseDto> PublishAsync(PublishLegalDocumentDto dto)
        {
            var version = NormalizeVersion(dto);
            await EnsureVersionIsAvailableAsync(dto.Type, version);
            var document = BuildDocument(dto, version);
            await legalDocumentRepository.PublishAsync(document);
            logger.LogInformation(
                "Published legal document {LegalDocumentId} of type {Type} version {Version}",
                document.LegalDocumentId,
                document.Type,
                document.Version);

            return ToResponse(document);
        }

        public async Task<LegalDocumentResponseDto> UpdateAsync(
            Guid legalDocumentId,
            UpdateLegalDocumentDto dto)
        {
            if (dto.Version is null && dto.Content is null)
                throw new BadRequestException("At least one field must be provided");

            var document = await GetEditableDocumentAsync(legalDocumentId);
            var version = NormalizeOptionalVersion(dto.Version);
            var content = NormalizeOptionalContent(dto.Content);

            if (version is not null)
                await EnsureVersionIsAvailableAsync(document.Type, version, legalDocumentId);

            ApplyChanges(document, version, content);
            await legalDocumentRepository.UpdateAsync(document);
            logger.LogInformation("Updated legal document {LegalDocumentId}", document.LegalDocumentId);

            return ToResponse(document);
        }

        private static string NormalizeVersion(PublishLegalDocumentDto dto)
        {
            if (!Enum.IsDefined(dto.Type))
                throw new BadRequestException("Invalid legal document type");

            var version = dto.Version.Trim();

            if (version.Length == 0)
                throw new BadRequestException("Version is required");

            if (version.Length > 20)
                throw new BadRequestException("Version must be at most 20 characters long");

            return version;
        }

        private static string? NormalizeOptionalVersion(string? version)
        {
            if (version is null)
                return null;

            var trimmed = version.Trim();

            if (trimmed.Length == 0)
                throw new BadRequestException("Version cannot be empty");

            if (trimmed.Length > 20)
                throw new BadRequestException("Version must be at most 20 characters long");

            return trimmed;
        }

        private static string? NormalizeOptionalContent(string? content)
        {
            if (content is null)
                return null;

            if (string.IsNullOrWhiteSpace(content))
                throw new BadRequestException("Content cannot be empty");

            return content;
        }

        private async Task<LegalDocument> GetEditableDocumentAsync(Guid legalDocumentId)
        {
            var document = await legalDocumentRepository.GetByIdAsync(legalDocumentId)
                ?? throw new NotFoundException("Legal document not found");

            if (await userConsentRepository.HasAnyForDocumentAsync(legalDocumentId))
                throw new ConflictException(
                    "This legal document already has consents and cannot be edited. Publish a new version instead");

            return document;
        }

        private static void ApplyChanges(LegalDocument document, string? version, string? content)
        {
            if (version is not null)
                document.Version = version;

            if (content is not null)
                document.Content = content;
        }

        private async Task EnsureVersionIsAvailableAsync(
            LegalDocumentType type,
            string version,
            Guid? excludeLegalDocumentId = null)
        {
            if (await legalDocumentRepository.VersionExistsAsync(type, version, excludeLegalDocumentId))
                throw new ConflictException("A legal document with this version already exists for this type");
        }

        private async Task<List<LegalDocument>> GetActiveDocumentsAsync(IReadOnlyCollection<Guid> legalDocumentIds)
        {
            var documents = await legalDocumentRepository.GetByIdsAsync(legalDocumentIds);

            if (documents.Count != legalDocumentIds.Count || documents.Any(document => !document.IsActive))
                throw new NotFoundException("One or more legal documents were not found or are not active");

            return documents;
        }

        public async Task<List<UserConsent>> PrepareRegistrationConsentsAsync(
            Guid userId,
            IReadOnlyCollection<Guid> legalDocumentIds,
            string? ipAddress)
        {
            EnsureValidIds(legalDocumentIds);

            var activeDocuments = await legalDocumentRepository.GetActiveAsync();

            if (activeDocuments.Count == 0)
                throw new BadRequestException("No active legal documents are available for acceptance");

            EnsureAllActiveDocumentsAccepted(activeDocuments, legalDocumentIds);

            return activeDocuments
                .Select(document => CreateConsent(userId, document.LegalDocumentId, ipAddress))
                .ToList();
        }

        private static LegalDocument BuildDocument(PublishLegalDocumentDto dto, string version) => new()
        {
            LegalDocumentId = Guid.NewGuid(),
            Type = dto.Type,
            Version = version,
            Content = dto.Content,
            PublishedAt = DateTime.UtcNow,
            IsActive = true
        };

        private static UserConsent CreateConsent(Guid userId, Guid legalDocumentId, string? ipAddress) => new()
        {
            UserConsentId = Guid.NewGuid(),
            UserId = userId,
            LegalDocumentId = legalDocumentId,
            AcceptedAt = DateTime.UtcNow,
            IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim()
        };

        private static void EnsureValidIds(IReadOnlyCollection<Guid>? legalDocumentIds)
        {
            if (legalDocumentIds is null || legalDocumentIds.Count == 0)
                throw new BadRequestException("At least one legal document must be accepted");

            if (legalDocumentIds.Distinct().Count() != legalDocumentIds.Count)
                throw new BadRequestException("Duplicate legal document IDs are not allowed");

            if (legalDocumentIds.Any(id => id == Guid.Empty))
                throw new BadRequestException("Legal document ID cannot be empty");
        }

        private static void EnsureAllActiveDocumentsAccepted(
            IReadOnlyCollection<LegalDocument> activeDocuments,
            IReadOnlyCollection<Guid> acceptedIds)
        {
            var active = activeDocuments.Select(document => document.LegalDocumentId).ToHashSet();
            var accepted = acceptedIds.ToHashSet();

            if (!active.IsSubsetOf(accepted) || !accepted.IsSubsetOf(active))
                throw new BadRequestException("All active legal documents must be accepted before registration");
        }

        private static LegalDocumentType ParseType(string type) =>
            type.Trim().ToLowerInvariant() switch
            {
                "privacy-policy" => LegalDocumentType.PrivacyPolicy,
                "terms-of-service" => LegalDocumentType.TermsOfService,
                _ => throw new BadRequestException("Invalid legal document type")
            };

        private static LegalDocumentResponseDto ToResponse(LegalDocument document) => new()
        {
            LegalDocumentId = document.LegalDocumentId,
            Type = document.Type.ToString(),
            Version = document.Version,
            Content = document.Content,
            PublishedAt = document.PublishedAt,
            IsActive = document.IsActive
        };

        private static UserConsentResponseDto ToConsentResponse(UserConsent consent) => new()
        {
            UserConsentId = consent.UserConsentId,
            LegalDocumentId = consent.LegalDocumentId,
            Type = consent.LegalDocument.Type.ToString(),
            Version = consent.LegalDocument.Version,
            AcceptedAt = consent.AcceptedAt,
            IpAddress = consent.IpAddress
        };
    }
}
