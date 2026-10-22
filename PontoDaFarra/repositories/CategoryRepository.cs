using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PontoDaFarra.context;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;

namespace PontoDaFarra.repositories
{
    public class CategoryRepository(AppDbContext context) : ICategoryRepository
    {
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }
        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            Category? category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            return category;
        }
        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            Category? category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name);
            return category;
        }
        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            EntityEntry<Category> entry = context.Categories.Update(category);
            await context.SaveChangesAsync();
            return entry.Entity;
        }
        public async Task<Category> DeleteCategoryAsync(Category category)
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return category;
        }
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await context.Categories
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<(List<Category> Items, int Total)> GetCategoriesWithProductsPagedAsync(
    int page, int pageSize, int maxProducts)
        {
            IQueryable<Category> query = context.Categories.AsNoTracking();

            int total = await query.CountAsync();

            List<Category> items = await query
                .OrderBy(c => c.Name)                  // ordem obrigatória para paginar
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Products
                    .OrderBy(p => p.Name)
                    .Take(maxProducts))
                .ToListAsync();

            return (items, total);
        }
    }
}