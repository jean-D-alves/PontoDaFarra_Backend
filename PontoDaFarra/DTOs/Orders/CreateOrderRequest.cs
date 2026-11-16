using System;
using System.Collections.Generic;
using PontoDaFarra.models;

namespace PontoDaFarra.DTOs.Orders
{
    public class CreateOrderRequest
    {
        public List<CreateOrderItemRequest> Items { get; set; } = new();
        public FulfillmentMethod FulfillmentMethod { get; set; }
    }
}
