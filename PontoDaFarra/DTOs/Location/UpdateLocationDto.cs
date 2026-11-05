using System.ComponentModel.DataAnnotations;

namespace PontoDaFarra.DTOs.Location
{
    public class UpdateLocationDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Neighborhood { get; set; }

        [MaxLength(150)]
        public string? Street { get; set; }

        [MaxLength(20)]
        public string? HouseNumber { get; set; }
    }
}
