namespace PontoDaFarra.DTOs.Location
{
    public class LocationResponseDto
    {
        public Guid LocationId { get; set; }
        public required string Name { get; set; }
        public required string State { get; set; }
        public required string City { get; set; }
        public required string Neighborhood { get; set; }
        public required string Street { get; set; }
        public required string HouseNumber { get; set; }
        public Guid UserId { get; set; }
    }
}
