using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.DTOs.Products;

namespace PontoDaFarra.services.Interface
{
    public interface IProductService
    {
        Task<ProductResponse> CreateProductAsync(CreateProductRequest dto);
        Task<ProductResponse> GetProductByIdAsync(Guid productId);
        Task<PagedResponse<ProductResponse>> SearchProductsAsync(SearchQuery search);
        Task<ProductResponse> UpdateProductAsync(Guid productId, CreateProductRequest dto);
        Task<ProductResponse> UpdateIsAvailableAsync(Guid productId);
        Task<ProductResponse> UpdateIsPromotionalAsync(PromotionalRequest dto, Guid productId);
        Task<ProductResponse> DeleteProductAsync(Guid productId);

    }
}