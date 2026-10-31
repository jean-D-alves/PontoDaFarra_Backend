using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.DTOs.Products;

namespace PontoDaFarra.DTOs.Category
{
    public class CategoryWithProductsPagedResponse
    {
        public Guid CategoryId { get; set; }
        public required string Name { get; set; }
        public required PagedResponse<ProductResponse> Products { get; set; }
    }
}
