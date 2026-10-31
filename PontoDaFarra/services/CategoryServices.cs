using System.Text.RegularExpressions;
using PontoDaFarra.DTOs.Category;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.DTOs.Products;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.services
{
    public partial class CategoryServices(
        ICategoryRepository categoryRepository) : ICategoryServices
    {
        private const int MaxProductsPerCategory = 5;
        private const int MaxProductsPageSize = 20;
        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest dto)
        {
            Category entry = new Category
            {
                Name = NormalizeName(dto.Name)
            };

            Category category = await categoryRepository.CreateCategoryAsync(entry);

            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name
            };
        }
        public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid categoryId)
        {
            Category? category = await categoryRepository.GetCategoryByIdAsync(categoryId);

            if (category is null)
                return null;

            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name
            };
        }
        public async Task<CategoryWithProductsPagedResponse?> GetCategoryByNameAsync(
            string name, int page, int pageSize)
        {
            string normalized = NormalizeName(name);
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, MaxProductsPageSize);
            var (category, products, total) =
                await categoryRepository.GetCategoryWithProductsPagedAsync(normalized, page, pageSize);

            if (category is null)
                return null;

            return new CategoryWithProductsPagedResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Products = BuildPagedProducts(products, page, pageSize, total)
            };
        }

        private static PagedResponse<ProductResponse> BuildPagedProducts(
            List<Product> products, int page, int pageSize, int total) =>
            new()
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Items = products.Select(product => new ProductResponse
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Value = product.Value,
                    IsAvailable = product.IsAvailable,
                    IsPromotional = product.IsPromotional,
                    ImageUrl = product.ImageUrl,
                    CategoryId = product.CategoryId
                }).ToList()
            };
        public async Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, CreateCategoryRequest dto)
        {
            Category? existingCategory = await categoryRepository.GetCategoryByIdAsync(categoryId);

            if (existingCategory is null)
                throw new ArgumentException("Category not found.");

            existingCategory.Name = NormalizeName(dto.Name);

            Category updatedCategory = await categoryRepository.UpdateCategoryAsync(existingCategory);

            return new CategoryResponse
            {
                CategoryId = updatedCategory.CategoryId,
                Name = updatedCategory.Name
            };
        }

        public async Task<CategoryResponse> DeleteCategoryAsync(Guid categoryId)
        {
            Category? existingCategory = await categoryRepository.GetCategoryByIdAsync(categoryId);

            if (existingCategory is null)
                throw new ArgumentException("Category not found.");

            Category deletedCategory = await categoryRepository.DeleteCategoryAsync(existingCategory);

            return new CategoryResponse
            {
                CategoryId = deletedCategory.CategoryId,
                Name = deletedCategory.Name
            };
        }
        public async Task<List<CategoryResponse>> GetAllCategoriesAsync()
        {
            List<Category> categories = await categoryRepository.GetAllCategoriesAsync();

            return categories.
            Select(category => new CategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name
            }).ToList();
        }
        public async Task<PagedResponse<CategoryWithProductsResponse>> GetCategoriesWithProductsPagedAsync(
            int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var (categories, total) =
                await categoryRepository.GetCategoriesWithProductsPagedAsync(page, pageSize, MaxProductsPerCategory);

            return new PagedResponse<CategoryWithProductsResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Items = categories.Select(category => new CategoryWithProductsResponse
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Products = category.Products?.Select(product => new ProductResponse
                    {
                        ProductId = product.ProductId,
                        Name = product.Name,
                        Description = product.Description,
                        Value = product.Value,
                        IsAvailable = product.IsAvailable,
                        IsPromotional = product.IsPromotional,
                        ImageUrl = product.ImageUrl,
                        CategoryId = product.CategoryId
                    }).ToArray()
                }).ToList()
            };
        }
        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty.");

            string normalized = name.Trim().ToLowerInvariant();

            if (!NameRegex().IsMatch(normalized))
                throw new ArgumentException(
                    "Name must be words separated by hyphens (e.g. 'nome-segundo-nome').");

            return normalized;
        }

        [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
        private static partial Regex NameRegex();
    }
}