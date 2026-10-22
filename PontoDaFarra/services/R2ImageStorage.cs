using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.services
{
    public class R2ImageStorage : IImageStorage
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;
        private readonly string _publicUrl;

        public R2ImageStorage(IConfiguration config)
        {
            var accountId = config["Storage:R2:AccountId"]!;
            _bucket = config["Storage:R2:Bucket"]!;
            _publicUrl = config["Storage:R2:PublicUrl"]!;

            _s3 = new AmazonS3Client(
                config["Storage:R2:AccessKey"],
                config["Storage:R2:SecretKey"],
                new AmazonS3Config
                {
                    ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
                    ForcePathStyle = true
                });
        }

        public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
        {
            var key = $"products/{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            await _s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                InputStream = content,
                ContentType = contentType,
                DisablePayloadSigning = true // necessário no R2
            }, ct);
            return key;
        }

        public Task DeleteAsync(string key, CancellationToken ct = default)
            => _s3.DeleteObjectAsync(_bucket, key, ct);

        public string GetPublicUrl(string key) => $"{_publicUrl}/{key}";
    }
}