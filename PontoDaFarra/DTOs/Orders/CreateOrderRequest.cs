using System;
using System.Collections.Generic;

namespace PontoDaFarra.DTOs.Orders
{
    public class CreateOrderRequest
    {
        public decimal TotalValue { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }
}
