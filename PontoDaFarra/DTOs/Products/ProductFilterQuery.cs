using System;

namespace PontoDaFarra.DTOs.Products
{
    public class ProductFilterQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Quando null, o filtro não é aplicado.
        public bool? IsAvailable { get; set; }
        public bool? IsPromotional { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
