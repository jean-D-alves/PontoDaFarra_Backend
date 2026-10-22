using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.models;

namespace PontoDaFarra.repositories.Interface
{
    public interface ICategoryRepository
    {
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category?> GetCategoryByIdAsync(Guid categoryId);
        Task<Category?> GetCategoryByNameAsync(string name);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<Category> DeleteCategoryAsync(Category category);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<(List<Category> Items, int Total)> GetCategoriesWithProductsPagedAsync(
    int page, int pageSize, int maxProducts);
    }
}