using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        private static ProductResponse ToResponse(Product product, IImageStorage imageStorage) =>
            new()
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Value = product.Value,
                IsAvailable = product.IsAvailable,
                IsPromotional = product.IsPromotional,
                ImageUrl = product.ImageUrl is null ? null : imageStorage.GetPublicUrl(product.ImageUrl),
                CategoryId = product.CategoryId
            };

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest dto)
        {
            if (dto.CategoryId == Guid.Empty)
                throw new BadRequestException("CategoryId is required.");

            string? imageKey = await TryUploadImageAsync(dto.Image);

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

                return ToResponse(product, imageStorage);
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

            return ToResponse(product, imageStorage);
        }
        public async Task<PagedResponse<ProductResponse>> SearchProductsAsync(SearchQuery search)
        {
            int page = Math.Max(search.page, 1);
            int pageSize = Math.Clamp(search.pageSize, 1, 50);

            var (items, total) = await productsRepository.SearchProductsAsync(search, page, pageSize);

            return new PagedResponse<ProductResponse>
            {
                Items = items.Select(product => ToResponse(product, imageStorage)).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<PagedResponse<ProductResponse>> GetAllProductsAsync(int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var (items, total) = await productsRepository.GetAllProductsAsync(page, pageSize);

            return new PagedResponse<ProductResponse>
            {
                Items = items.Select(product => ToResponse(product, imageStorage)).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<ProductResponse> UpdateProductAsync(Guid productId, UpdateProductRequest dto)
        {
            var product = await productsRepository.GetProductByIdAsync(productId);
            if (product is null)
                throw new KeyNotFoundException("Produto não encontrado.");

            if (dto.CategoryId == Guid.Empty)
                throw new BadRequestException("CategoryId is required.");

            string? oldImageKey = product.ImageUrl;
            string? newImageKey = await TryUploadImageAsync(dto.Image);

            ApplyChanges(product, dto, newImageKey);
            await SaveWithImageCleanupAsync(product, oldImageKey, newImageKey);

            return ToResponse(product, imageStorage);
        }

        private async Task SaveWithImageCleanupAsync(Product product, string? oldImageKey, string? newImageKey)
        {
            try
            {
                await productsRepository.UpdateProductAsync(product);
            }
            catch
            {
                if (newImageKey is not null)
                    await imageStorage.DeleteAsync(newImageKey);
                throw;
            }

            if (newImageKey is not null && oldImageKey is not null)
                await imageStorage.DeleteAsync(oldImageKey);
        }

        private static void ApplyChanges(Product product, UpdateProductRequest dto, string? newImageKey)
        {
            if (dto.Name is not null) product.Name = dto.Name;
            if (dto.Description is not null) product.Description = dto.Description;
            if (dto.Value.HasValue) product.Value = dto.Value.Value;
            if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;
            if (newImageKey is not null) product.ImageUrl = newImageKey;
        }

        private async Task<string?> TryUploadImageAsync(IFormFile? image)
        {
            if (image is not { Length: > 0 })
                return null;

            if (image.Length > MaxImageSize)
                throw new BadRequestException("The image must be no larger than 5 MB.");

            await using var stream = image.OpenReadStream();

            var format = ImageSignatureValidator.Detect(stream)
                ?? throw new BadRequestException("Invalid image content. Use JPEG, PNG, or WebP.");

            if (stream.CanSeek) stream.Position = 0;

            return await imageStorage.UploadAsync(stream, format.Extension, format.ContentType);
        }

        public async Task<ProductResponse> UpdateIsAvailableAsync(Guid productId)
        {
            var product = await productsRepository.GetProductByIdAsync(productId);
            if (product is null)
                throw new KeyNotFoundException("Produto não encontrado.");

            product.IsAvailable = !product.IsAvailable;

            await productsRepository.UpdateProductAsync(product);

            return ToResponse(product, imageStorage);
        }

        public async Task<ProductResponse> UpdateIsPromotionalAsync(PromotionalRequest dto, Guid productId)
        {
            var product = await productsRepository.GetProductByIdAsync(productId);
            if (product is null)
                throw new KeyNotFoundException("Produto não encontrado.");
            product.IsPromotional = dto.isPromotional;
            product.Value = dto.PromotionalValue;

            await productsRepository.UpdateProductAsync(product);

            return ToResponse(product, imageStorage);
        }

        public async Task<ProductResponse> DeleteProductAsync(Guid productId)
        {
            var product = await productsRepository.GetProductByIdAsync(productId);
            if (product is null)
                throw new KeyNotFoundException("Produto não encontrado.");

            await productsRepository.DeleteProductAsync(product);

            if (product.ImageUrl is not null)
                await imageStorage.DeleteAsync(product.ImageUrl);

            return ToResponse(product, imageStorage);
        }
    }
}