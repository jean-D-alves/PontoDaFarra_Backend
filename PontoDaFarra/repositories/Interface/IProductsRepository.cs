using PontoDaFarra.models;

namespace PontoDaFarra.repositories.Interface
{
    public interface IProductsRepository
    {
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> GetProductByIdAsync(Guid productId);
        Task<(List<Product> Items, int Total)> SearchProductsAsync(string term, int page, int pageSize);
        Task<Product> UpdateProductAsync(Product product);
        Task<Product> DeleteProductAsync(Product product);
    }
}