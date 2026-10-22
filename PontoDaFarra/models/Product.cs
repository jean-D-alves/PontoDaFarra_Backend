using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.models
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public required String Name { get; set; }
        public String? Description { get; set; }
        public required String Value { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsPromotional { get; set; } = false;
        public String? ImageUrl { get; set; }
        public required Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}