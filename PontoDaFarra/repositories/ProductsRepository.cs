using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PontoDaFarra.context;
using PontoDaFarra.DTOs.Products;
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


        public async Task<(List<Product> Items, int Total)> SearchProductsAsync(
            SearchQuery filter, int page, int pageSize)
        {
            IQueryable<Product> query = context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.name))
            {
                string pattern = $"%{EscapeLike(filter.name.Trim())}%";
                query = query.Where(p => EF.Functions.ILike(p.Name, pattern)
                    || (p.Description != null && EF.Functions.ILike(p.Description, pattern)));
            }

            if (!filter.IncludeUnavailable)
                query = query.Where(p => p.IsAvailable);

            if (filter.OnlyPromos)
                query = query.Where(p => p.IsPromotional);

            if (filter.Below.HasValue)
                query = query.Where(p => p.Value <= filter.Below.Value);

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