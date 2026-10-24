namespace PontoDaFarra.services.Interface
{
    public interface IApiKeyService
    {
        bool IsValid(string? providedApiKey);
    }
}
