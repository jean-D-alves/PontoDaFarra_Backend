using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PontoDaFarra.models;

namespace PontoDaFarra.repositories.Interface
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<List<Order>> GetOrdersByUserIdAsync(Guid userId, int page, int pageSize);
        Task<(List<Order> Items, int Total)> GetAllOrdersAsync(int page, int pageSize);
        Task<Order?> GetOrderByIdAsync(Guid orderId);
        Task DeleteOrderAsync(Order order);
        Task<List<(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, Guid OrderId)>> GetItemsForChartAsync(DateTime from, DateTime to);
    }
}
