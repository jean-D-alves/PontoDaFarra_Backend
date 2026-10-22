using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PontoDaFarra.context;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;

namespace PontoDaFarra.repositories
{
    public class ProductsRepository(AppDbContext context) : IProductsRepository
    {

        private static string EscapeLike(string value) =>
            value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        public async Task<Product> CreateProductAsync(Product product)
        {
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return product;
        }
        public async Task<Product?> GetProductByIdAsync(Guid productId)
        {
            Product? product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            return product;
        }


        public async Task<(List<Product> Items, int Total)> SearchProductsAsync(string term, int page, int pageSize)
        {
            string pattern = $"%{EscapeLike(term.Trim())}%";

            IQueryable<Product> query = context
            .Products
            .AsNoTracking()
            .Where(p => EF.Functions.ILike(p.Name, pattern)
            || (p.Description != null && EF.Functions.ILike(p.Description, pattern)));

            int total = await query.CountAsync();

            List<Product> items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            return (items, total);
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            context.Products.Update(product);
            await context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> DeleteProductAsync(Product product)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return product;
        }
    }
}