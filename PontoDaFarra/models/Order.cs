using System;
using System.Collections.Generic;

namespace PontoDaFarra.models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public Guid? UserId { get; set; }
        public required string CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public User? User { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
