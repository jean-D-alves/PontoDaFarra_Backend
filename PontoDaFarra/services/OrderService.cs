using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.DTOs.Orders;
using PontoDaFarra.Exceptions;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.services
{
    public class OrderService(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductsRepository productsRepository) : IOrderService
    {
        public async Task<OrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                throw new BadRequestException("Items cannot be empty");
            var user = await userRepository.GetUserByIdAsync(userId)
                ?? throw new NotFoundException("User not found");
            if (string.IsNullOrWhiteSpace(user.Name))
                throw new BadRequestException("User name is required");
            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                throw new BadRequestException("User phone number is required");
            var grouped = request.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();
            var orderItems = new List<OrderItem>();
            decimal totalValue = 0;
            foreach (var item in grouped)
            {
                if (item.Quantity <= 0)
                    throw new BadRequestException("Quantity must be greater than zero");
                var product = await productsRepository.GetProductByIdAsync(item.ProductId)
                    ?? throw new BadRequestException("Product not found");
                if (product.Value <= 0)
                    throw new BadRequestException("Product price must be greater than zero");
                orderItems.Add(new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.Value
                });
                totalValue += item.Quantity * product.Value;
            }
            if (!Enum.IsDefined(typeof(FulfillmentMethod), request.FulfillmentMethod))
                throw new BadRequestException("Invalid fulfillment method");
            int deliveryFee = 0;
            if (totalValue <= 0)
                throw new BadRequestException("TotalValue must be greater than zero");
            if (totalValue > 10000)
                throw new BadRequestException("TotalValue must be less than or equal to 10,000");
            if (request.FulfillmentMethod == FulfillmentMethod.Delivery && totalValue < 25)
                deliveryFee = 3;
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = user.UserId,
                CustomerName = user.Name.Trim(),
                CustomerPhone = string.IsNullOrWhiteSpace(user.PhoneNumber) ? null : user.PhoneNumber.Trim(),
                TotalValue = totalValue,
                FulfillmentMethod = request.FulfillmentMethod,
                DeliveryFee = deliveryFee,
                CreatedAt = DateTime.UtcNow,
                Items = orderItems
            };
            var created = await orderRepository.CreateOrderAsync(order);
            return MapOrder(created);
        }

        public async Task<List<OrderListItemResponse>> GetMyOrdersAsync(Guid userId, int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var orders = await orderRepository.GetOrdersByUserIdAsync(userId, page, pageSize);
            return orders.Select(MapOrderList).ToList();
        }

        public async Task DeleteOrderAsync(Guid callerId, bool isAdmin, Guid orderId)
        {
            var order = await orderRepository.GetOrderByIdAsync(orderId)
                ?? throw new NotFoundException("Order not found");
            if (!isAdmin && order.UserId != callerId)
                throw new ForbiddenException("Not allowed to delete this order");
            await orderRepository.DeleteOrderAsync(order);
        }

        public async Task<PagedResponse<AdminOrderResponse>> GetAllOrdersAsync(int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var result = await orderRepository.GetAllOrdersAsync(page, pageSize);
            return new PagedResponse<AdminOrderResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = result.Total,
                Items = result.Items.Select(MapAdminOrder).ToList()
            };
        }

        public async Task<OrderChartResponse> GetChartAsync()
        {
            var to = DateTime.UtcNow;
            var from = to.AddDays(-30);
            var items = await orderRepository.GetItemsForChartAsync(from, to);
            var byQuantity = items
                .GroupBy(i => new { i.ProductId, i.ProductName })
                .Select(g => new TopProductByQuantity
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(6)
                .ToList();
            var byValue = items
                .GroupBy(i => new { i.ProductId, i.ProductName })
                .Select(g => new TopProductByValue
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalValue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalValue)
                .Take(5)
                .ToList();
            var byOrderCount = items
                .GroupBy(i => new { i.ProductId, i.ProductName, i.OrderId })
                .GroupBy(g => new { g.Key.ProductId, g.Key.ProductName })
                .Select(g => new TopProductByOrderCount
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.OrderCount)
                .Take(6)
                .ToList();
            return new OrderChartResponse
            {
                ByQuantity = byQuantity,
                ByValue = byValue,
                ByOrderCount = byOrderCount
            };
        }

        public async Task<OrderMoreResponse> GetProductsMoreOrder()
        {
            var to = DateTime.UtcNow;
            var from = to.AddDays(-30);
            var items = await orderRepository.GetItemsForChartAsync(from, to);
            var byOrderCount = items
                .GroupBy(i => new { i.ProductId, i.ProductName, i.OrderId })
                .GroupBy(g => new { g.Key.ProductId, g.Key.ProductName })
                .Select(g => new TopProductByOrderCount
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.OrderCount)
                .Take(6)
                .ToList();
            return new OrderMoreResponse
            {
                ByOrderCount = byOrderCount
            };
        }

        private static OrderResponse MapOrder(Order order) => new()
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            TotalValue = order.TotalValue,
            FulfillmentMethod = order.FulfillmentMethod,
            DeliveryFee = order.DeliveryFee,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()

        };

        private static OrderListItemResponse MapOrderList(Order order) => new()
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            TotalValue = order.TotalValue,
            FulfillmentMethod = order.FulfillmentMethod,
            DeliveryFee = order.DeliveryFee,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        private static AdminOrderResponse MapAdminOrder(Order order) => new()
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            TotalValue = order.TotalValue,
            FulfillmentMethod = order.FulfillmentMethod,
            DeliveryFee = order.DeliveryFee,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}
