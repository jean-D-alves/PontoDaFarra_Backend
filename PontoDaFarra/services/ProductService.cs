using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.DTOs.Products;
using PontoDaFarra.Exceptions;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.services
{
    public class ProductService(
        IProductsRepository productsRepository,
        IImageStorage imageStorage) : IProductService
    {
        private const long MaxImageSize = 5 * 1024 * 1024;

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest dto)
        {
            if (dto.CategoryId == Guid.Empty)
                throw new BadRequestException("CategoryId is required.");

            string? imageKey = null;

            if (dto.Image is { Length: > 0 })
            {
                if (dto.Image.Length > MaxImageSize)
                    throw new BadRequestException("The image must be no larger than 5 MB.");

                await using var stream = dto.Image.OpenReadStream();

                var format = ImageSignatureValidator.Detect(stream)
                    ?? throw new BadRequestException("Invalid image content. Use JPEG, PNG, or WebP.");

                if (stream.CanSeek) stream.Position = 0;

                imageKey = await imageStorage.UploadAsync(stream, format.Extension, format.ContentType);
            }

            try
            {
                var product = new Product
                {
                    ProductId = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    Value = dto.Value,
                    ImageUrl = imageKey,
                    CategoryId = dto.CategoryId
                };

                await productsRepository.CreateProductAsync(product);

                return new ProductResponse
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Value = product.Value,
                    ImageUrl = imageKey is null ? null : imageStorage.GetPublicUrl(imageKey),
                    CategoryId = product.CategoryId
                };
            }
            catch
            {
                if (imageKey is not null)
                    await imageStorage.DeleteAsync(imageKey);
                throw;
            }
        }
        public async Task<ProductResponse> GetProductByIdAsync(Guid productId)
        {
            var product = await productsRepository.GetProductByIdAsync(productId);
            if (product is null)
                throw new KeyNotFoundException("Produto não encontrado.");

            return new ProductResponse
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Value = product.Value,
                ImageUrl = product.ImageUrl is null ? null : imageStorage.GetPublicUrl(product.ImageUrl),
                CategoryId = product.CategoryId
            };
        }
        public async Task<PagedResponse<ProductResponse>> SearchProductsAsync(string name, int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var (items, total) = await productsRepository.SearchProductsAsync(name ?? string.Empty, page, pageSize);

            return new PagedResponse<ProductResponse>
            {
                Items = items.Select(product => new ProductResponse
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Value = product.Value,
                    ImageUrl = product.ImageUrl is null ? null : imageStorage.GetPublicUrl(product.ImageUrl),
                    CategoryId = product.CategoryId
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<ProductResponse> UpdateProductAsync(Guid productId, CreateProductRequest dto)
        {
            var product = await productsRepository.GetProductByIdAsync(productId);
            if (product is null)
                throw new KeyNotFoundException("Produto não encontrado.");

            if (dto.CategoryId == Guid.Empty)
                throw new BadRequestException("CategoryId is required.");

            string? imageKey = product.ImageUrl;

            if (dto.Image is { Length: > 0 })
            {
                if (dto.Image.Length > MaxImageSize)
                    throw new BadRequestException("The image must be no larger than 5 MB.");

                await using var stream = dto.Image.OpenReadStream();

                var format = ImageSignatureValidator.Detect(stream)
                    ?? throw new BadRequestException("Invalid image content. Use JPEG, PNG, or WebP.");

                if (stream.CanSeek) stream.Position = 0;

                imageKey = await imageStorage.UploadAsync(stream, format.Extension, format.ContentType);

                if (product.ImageUrl is not null)
                    await imageStorage.DeleteAsync(product.ImageUrl);
            }

            product.Name = dto.Name ?? product.Name;
            product.Description = dto.Description ?? product.Description;
            product.Value = dto.Value ?? product.Value;
            product.CategoryId = dto.CategoryId;
            product.ImageUrl = imageKey;

            await productsRepository.UpdateProductAsync(product);

            return new ProductResponse
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Value = product.Value,
                ImageUrl = imageKey is null ? null : imageStorage.GetPublicUrl(imageKey),
                CategoryId = product.CategoryId
            };
        }
        public async Task<ProductResponse> DeleteProductAsync(Guid productId)
        {
            var product = await productsRepository.GetProductByIdAsync(productId);
            if (product is null)
                throw new KeyNotFoundException("Produto não encontrado.");

            await productsRepository.DeleteProductAsync(product);

            if (product.ImageUrl is not null)
                await imageStorage.DeleteAsync(product.ImageUrl);

            return new ProductResponse
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Value = product.Value,
                ImageUrl = product.ImageUrl is null ? null : imageStorage.GetPublicUrl(product.ImageUrl),
                CategoryId = product.CategoryId
            };
        }
    }
}