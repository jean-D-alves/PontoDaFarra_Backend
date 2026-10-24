using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.Filters
{
    public sealed class ApiKeyFilter(IApiKeyService apiKeyService) : IAsyncActionFilter
    {
        private const string HeaderName = "X-Api-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var providedApiKey = context.HttpContext.Request.Headers[HeaderName].ToString();

            if (!apiKeyService.IsValid(providedApiKey))
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Chave de API inválida ou ausente." });
                return;
            }

            await next();
        }
    }
}
