using System;

namespace PontoDaFarra.DTOs.Orders
{
    public class OrderItemResponse
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
