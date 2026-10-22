using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.DTOs.Category
{
    public class CategoryResponse
    {
        public Guid CategoryId { get; set; }
        public required String Name { get; set; }
    }
}