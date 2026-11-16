using System;
using System.Collections.Generic;

namespace PontoDaFarra.DTOs.Orders
{
    public class TopProductByQuantity
    {
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class TopProductByValue
    {
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal TotalValue { get; set; }
    }

    public class TopProductByOrderCount
    {
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int OrderCount { get; set; }
    }

    public class OrderChartResponse
    {
        public List<TopProductByQuantity> ByQuantity { get; set; } = new();
        public List<TopProductByValue> ByValue { get; set; } = new();
        public List<TopProductByOrderCount> ByOrderCount { get; set; } = new();
    }
}
