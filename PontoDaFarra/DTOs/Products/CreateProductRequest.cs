using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.DTOs.Products
{
    public class CreateProductRequest
    {
        [Required, StringLength(120)] 
        public required String Name { get; set; }
        [StringLength(240)] 
        public String? Description { get; set; }
        [Range(0, 99999999.99)]
        public decimal Value { get; set; }
        public IFormFile? Image { get; set; }
        public required Guid CategoryId { get; set; }
    }
}