using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;

namespace PontoDaFarra.services
{
    public class LegalDocumentSeeder(
        ILegalDocumentRepository legalDocumentRepository,
        ILogger<LegalDocumentSeeder> logger)
    {
        private const string Version = "1.0";

        private static readonly LegalDocumentSeed[] Seeds =
        [
            new(
                LegalDocumentType.TermsOfService,
                "PontoDaFarra.Resources.Legal.termos-v1.0.md",
                new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc)),
            new(
                LegalDocumentType.PrivacyPolicy,
                "PontoDaFarra.Resources.Legal.politica-privacidade-v1.0.md",
                new DateTime(2026, 11, 5, 0, 0, 0, DateTimeKind.Utc))
        ];

        public async Task SeedAsync()
        {
            var existingTypes = (await legalDocumentRepository.GetAllAsync())
                .Select(document => document.Type)
                .ToHashSet();

            foreach (var seed in Seeds)
                await SeedDocumentAsync(seed, existingTypes);
        }

        private async Task SeedDocumentAsync(
            LegalDocumentSeed seed,
            HashSet<LegalDocumentType> existingTypes)
        {
            if (existingTypes.Contains(seed.Type))
            {
                logger.LogInformation(
                    "Documento legal {Type} já existe; seed ignorado.", seed.Type);
                return;
            }

            var document = new LegalDocument
            {
                LegalDocumentId = Guid.NewGuid(),
                Type = seed.Type,
                Version = Version,
                Content = ReadContent(seed.ResourceName),
                PublishedAt = seed.PublishedAt,
                IsActive = true
            };

            await legalDocumentRepository.PublishAsync(document);

            logger.LogInformation(
                "Documento legal {Type} versão {Version} semeado.", seed.Type, Version);
        }

        private static string ReadContent(string resourceName)
        {
            using var stream = typeof(LegalDocumentSeeder).Assembly
                .GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Recurso embutido '{resourceName}' não encontrado.");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private sealed record LegalDocumentSeed(
            LegalDocumentType Type,
            string ResourceName,
            DateTime PublishedAt);
    }
}
