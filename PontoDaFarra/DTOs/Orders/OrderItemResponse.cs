using System;

namespace PontoDaFarra.DTOs.Orders
{
    public class OrderItemResponse
    {
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
