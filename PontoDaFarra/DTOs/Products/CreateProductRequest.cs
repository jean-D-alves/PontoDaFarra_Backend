using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.DTOs.Products
{
    public class CreateProductRequest
    {
        public required String Name { get; set; }
        public String? Description { get; set; }
        public required String Value { get; set; }
        public IFormFile? Image { get; set; }
        public required Guid CategoryId { get; set; }
    }
}