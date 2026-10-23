using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.DTOs.Products
{
    public class PromotionalRequest
    {
        public decimal PromotionalValue { get; set; }
        public bool isPromotional { get; set; }
    }
}