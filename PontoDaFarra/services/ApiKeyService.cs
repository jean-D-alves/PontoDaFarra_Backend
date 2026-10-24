using System.Security.Cryptography;
using System.Text;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly string? _expectedApiKey;

        public ApiKeyService(IConfiguration configuration)
        {
            _expectedApiKey = configuration["ApiKey"];
        }

        public bool IsValid(string? providedApiKey)
        {
            if (string.IsNullOrWhiteSpace(_expectedApiKey) || string.IsNullOrEmpty(providedApiKey))
                return false;

            var expectedBytes = Encoding.UTF8.GetBytes(_expectedApiKey);
            var providedBytes = Encoding.UTF8.GetBytes(providedApiKey);

            return CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
        }
    }
}
