using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.DTOs;
using PontoDaFarra.DTOs.Category;
using PontoDaFarra.DTOs.Global;

namespace PontoDaFarra.services.Interface
{
    public interface ICategoryServices
    {
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest dto);
        Task<CategoryResponse?> GetCategoryByIdAsync(Guid categoryId);
        Task<CategoryResponse?> GetCategoryByNameAsync(string name);
        Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, CreateCategoryRequest dto);
        Task<CategoryResponse> DeleteCategoryAsync(Guid categoryId);
        Task<List<CategoryResponse>> GetAllCategoriesAsync();
        Task<PagedResponse<CategoryWithProductsResponse>> GetCategoriesWithProductsPagedAsync(
            int page, int pageSize);
    }
}