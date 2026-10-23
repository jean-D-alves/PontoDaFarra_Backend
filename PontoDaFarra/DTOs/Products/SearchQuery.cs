using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.DTOs.Products
{
    public class SearchQuery
    {
        public required string name { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public bool IncludeUnavailable {get; set;} = false;
        public bool OnlyPromos {get; set;} = false;
        public decimal? Below {get; set;}
    }
}