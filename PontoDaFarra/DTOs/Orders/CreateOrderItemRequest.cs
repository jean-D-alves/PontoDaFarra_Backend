using System;

namespace PontoDaFarra.DTOs.Orders
{
    public class CreateOrderItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
