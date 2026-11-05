using System.ComponentModel.DataAnnotations;

namespace PontoDaFarra.DTOs.Location
{
    public class CreateLocationDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public required string State { get; set; }

        [Required]
        [MaxLength(100)]
        public required string City { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Neighborhood { get; set; }

        [Required]
        [MaxLength(150)]
        public required string Street { get; set; }

        [Required]
        [MaxLength(20)]
        public required string HouseNumber { get; set; }
    }
}
