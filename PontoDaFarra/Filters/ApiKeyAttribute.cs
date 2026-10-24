using Microsoft.AspNetCore.Mvc;

namespace PontoDaFarra.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ApiKeyAttribute : TypeFilterAttribute
    {
        public ApiKeyAttribute() : base(typeof(ApiKeyFilter))
        {
        }
    }
}
