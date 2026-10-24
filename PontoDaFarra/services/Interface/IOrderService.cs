using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.DTOs.Orders;

namespace PontoDaFarra.services.Interface
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest request);
        Task<List<OrderListItemResponse>> GetMyOrdersAsync(Guid userId, int page, int pageSize);
        Task DeleteOrderAsync(Guid callerId, bool isAdmin, Guid orderId);
        Task<PagedResponse<AdminOrderResponse>> GetAllOrdersAsync(int page, int pageSize);
        Task<OrderChartResponse> GetChartAsync();
    }
}
