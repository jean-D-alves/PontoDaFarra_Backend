using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.services.Interface
{
    public interface IImageStorage
    {
        Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);
        Task DeleteAsync(string key, CancellationToken ct = default);
        string GetPublicUrl(string key);
    }
}