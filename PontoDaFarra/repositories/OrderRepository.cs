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
    public class OrderRepository(AppDbContext context) : IOrderRepository
    {
        public async Task<Order> CreateOrderAsync(Order order)
        {
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            return order;
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId, int page, int pageSize)
        {
            return await context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<(List<Order> Items, int Total)> GetAllOrdersAsync(int page, int pageSize)
        {
            var query = context.Orders
                .AsNoTracking()
                .Include(o => o.Items);
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, total);
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task DeleteOrderAsync(Order order)
        {
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
        }

        public async Task<List<(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, Guid OrderId)>> GetItemsForChartAsync(DateTime from, DateTime to)
        {
            var result = await context.OrderItems
                .AsNoTracking()
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to)
                .Select(oi => new ValueTuple<Guid, string, int, decimal, Guid>(oi.ProductId, oi.Product.Name, oi.Quantity, oi.UnitPrice, oi.OrderId))
                .ToListAsync();
            return result;
        }
    }
}
