using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.DTOs.Orders
{
    public class OrderMoreResponse
    {
        public List<TopProductByOrderCount> ByOrderCount { get; set; } = new();
    }
}