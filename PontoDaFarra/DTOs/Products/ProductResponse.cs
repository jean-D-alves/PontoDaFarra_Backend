using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.DTOs.Products
{
    public class ProductResponse
    {
        public Guid ProductId { get; set; }
        public required String Name { get; set; }
        public String? Description { get; set; }
        public decimal Value { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsPromotional { get; set; } = false;
        public String? ImageUrl { get; set; }
        public required Guid CategoryId { get; set; }
    }
}