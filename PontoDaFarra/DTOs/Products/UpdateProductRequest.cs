using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PontoDaFarra.DTOs.Products
{
    public class UpdateProductRequest
    {
        [StringLength(120)]
        public string? Name { get; set; }
        [StringLength(240)]
        public string? Description { get; set; }
        public decimal? Value { get; set; }
        public IFormFile? Image { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
