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
        public required String Description { get; set; }
        public required String Value { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsPromotional { get; set; } = false;
        public required String ImageUrl { get; set; }
        public required Guid CategoryId { get; set; }
    }
}