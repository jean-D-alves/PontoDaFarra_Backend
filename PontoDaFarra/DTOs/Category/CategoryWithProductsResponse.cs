using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.DTOs.Products;

namespace PontoDaFarra.DTOs.Category
{
    public class CategoryWithProductsResponse
    {
        public Guid CategoryId { get; set; }
        public required string Name { get; set; }
        public ProductResponse[]? Products { get; set; }
    }
}