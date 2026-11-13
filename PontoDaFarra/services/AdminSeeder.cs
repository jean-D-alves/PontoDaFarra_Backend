using System.ComponentModel.DataAnnotations;
using PontoDaFarra.Exceptions;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;

namespace PontoDaFarra.services
{
    public class AdminSeeder(
        IConfiguration configuration,
        IUserRepository userRepository,
        ILegalDocumentRepository legalDocumentRepository,
        ILogger<AdminSeeder> logger)
    {
        private const string AdminRole = "Admin";
        private static readonly EmailAddressAttribute EmailValidator = new();

        public async Task SeedAsync()
        {
            if (await userRepository.HasAdminAsync())
            {
                logger.LogInformation("Já existe admin; seed do admin inicial ignorado.");
                return;
            }

            var credentials = ReadCredentials();

            if (credentials is null)
            {
                LogMissingConfiguration();
                return;
            }

            await CreateAdminIfAllowedAsync(credentials);
        }

        private void LogMissingConfiguration()
        {
            // Decisão: não falhar o startup. Em produção, a ausência do admin
            // é um erro de configuração operacional; derrubar a API não cria
            // o admin e impede inclusive o diagnóstico pela aplicação.
            // O aviso deve ser monitorado e corrigido com as variáveis + restart.
            logger.LogWarning(
                "Nenhum admin existe e ADMIN_EMAIL, ADMIN_NAME ou ADMIN_PASSWORD não estão configurados. Nenhum admin inicial será criado.");
        }

        private async Task CreateAdminIfAllowedAsync(AdminCredentials credentials)
        {
            if (!IsValid(credentials))
                return;

            if (await userRepository.GetUserByEmailAsync(credentials.Email) is not null)
            {
                logger.LogWarning(
                    "Já existe usuário com o e-mail de ADMIN_EMAIL, mas sem role Admin. Promoção automática não é permitida; seed do admin inicial ignorado.");
                return;
            }

            var admin = await BuildAdminAsync(credentials);

            if (!await userRepository.TryCreateAdminAsync(admin))
                logger.LogWarning(
                    "O admin inicial não foi criado: o e-mail já está em uso ou outra instância criou um admin ao mesmo tempo. Seed ignorado.");
            else
                logger.LogInformation("Admin inicial criado.");
        }

        private AdminCredentials? ReadCredentials()
        {
            var email = configuration["ADMIN_EMAIL"]?.Trim();
            var name = configuration["ADMIN_NAME"]?.Trim();
            var password = configuration["ADMIN_PASSWORD"];
            var phone = configuration["ADMIN_PHONE"]?.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
                return null;

            return new AdminCredentials(email.ToLowerInvariant(), name, password, phone ?? string.Empty);
        }

        private bool IsValid(AdminCredentials credentials) =>
            ValidateEmail(credentials.Email)
            && ValidateName(credentials.Name)
            && ValidatePassword(credentials.Password);

        private bool ValidateEmail(string email)
        {
            if (EmailValidator.IsValid(email))
                return true;

            logger.LogWarning("ADMIN_EMAIL inválido. Admin inicial não criado.");
            return false;
        }

        private bool ValidateName(string name)
        {
            if (name.Length <= 80)
                return true;

            logger.LogWarning("ADMIN_NAME excede 80 caracteres. Admin inicial não criado.");
            return false;
        }

        private bool ValidatePassword(string password)
        {
            try
            {
                PasswordService.ValidatePassword(password);
                return true;
            }
            catch (BadRequestException exception)
            {
                logger.LogWarning(
                    "ADMIN_PASSWORD inválida: {Reason}. Admin inicial não criado.",
                    exception.Message);
                return false;
            }
        }

        private async Task<User> BuildAdminAsync(AdminCredentials credentials)
        {
            var userId = Guid.NewGuid();
            var activeDocuments = await legalDocumentRepository.GetActiveAsync();

            if (activeDocuments.Count == 0)
            {
                logger.LogWarning(
                    "Nenhum documento legal ativo encontrado. O admin inicial será criado sem consentimentos.");
            }

            return CreateAdmin(credentials, userId, activeDocuments);
        }

        private static User CreateAdmin(
            AdminCredentials credentials,
            Guid userId,
            IReadOnlyCollection<LegalDocument> activeDocuments) => new()
            {
                UserId = userId,
                Name = credentials.Name,
                Email = credentials.Email,
                Password = PasswordService.HashPassword(credentials.Password),
                PhoneNumber = credentials.Phone,
                Role = AdminRole,
                Consents = BuildConsents(userId, activeDocuments)
            };

        private static List<UserConsent> BuildConsents(
            Guid userId,
            IReadOnlyCollection<LegalDocument> activeDocuments) =>
            activeDocuments
                .Select(document => new UserConsent
                {
                    UserConsentId = Guid.NewGuid(),
                    UserId = userId,
                    LegalDocumentId = document.LegalDocumentId,
                    AcceptedAt = DateTime.UtcNow
                })
                .ToList();

        private sealed record AdminCredentials(
            string Email,
            string Name,
            string Password,
            string Phone);
    }
}
