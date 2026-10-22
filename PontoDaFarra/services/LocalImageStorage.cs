using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.Exceptions;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.services
{
    public class LocalImageStorage : IImageStorage
    {
        private static readonly HashSet<string> AllowedExtensions = [".jpg", ".png", ".webp"];

        private readonly string _root;
        private readonly string _baseUrl;

        public LocalImageStorage(IConfiguration config)
        {
            _root = config["Storage:Local:Path"] ?? "uploads";
            _baseUrl = config["Storage:Local:BaseUrl"] ?? "http://localhost:5000/uploads";
            Directory.CreateDirectory(_root);
        }

        public async Task<string> UploadAsync(Stream content, string extension, string contentType, CancellationToken ct = default)
        {
            if (!AllowedExtensions.Contains(extension))
                throw new BadRequestException("Invalid image extension.");

            var key = $"{Guid.NewGuid()}{extension}";
            await using var file = File.Create(Path.Combine(_root, key));
            await content.CopyToAsync(file, ct);
            return key;
        }

        public Task DeleteAsync(string key, CancellationToken ct = default)
        {
            var path = Path.Combine(_root, key);
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }

        public string GetPublicUrl(string key) => $"{_baseUrl}/{key}";
    }
}