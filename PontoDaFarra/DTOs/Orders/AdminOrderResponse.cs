using System;
using System.Collections.Generic;
using PontoDaFarra.models;

namespace PontoDaFarra.DTOs.Orders
{
    public class AdminOrderResponse
    {
        public Guid OrderId { get; set; }
        public Guid? UserId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? CustomerPhone { get; set; }
        public decimal TotalValue { get; set; }
        public FulfillmentMethod FulfillmentMethod { get; set; }
        public int DeliveryFee { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
