using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.models
{
    public class Category
    {
        public Guid CategoryId { get; set; }
        public required String Name { get; set; }
        public List<Product> Products { get; set; } = [];
    }
}