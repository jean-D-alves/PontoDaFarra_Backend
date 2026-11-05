using PontoDaFarra.DTOs.Location;
using PontoDaFarra.Exceptions;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.services
{
    public class LocationService(ILocationRepository locationRepository) : ILocationService
    {
        private const int MaxLocationsPerUser = 3;

        public async Task<LocationResponseDto> CreateLocationAsync(Guid userId, CreateLocationDto dto)
        {
            if (await locationRepository.CountByUserIdAsync(userId) >= MaxLocationsPerUser)
                throw new ConflictException($"A user can have at most {MaxLocationsPerUser} locations");

            Location location = BuildLocation(userId, dto);

            if (await locationRepository.NameExistsAsync(userId, location.Name))
                throw new ConflictException("A location with this name already exists");

            return Map(await locationRepository.CreateLocationAsync(location));
        }

        public async Task<List<LocationResponseDto>> GetLocationsAsync(Guid userId)
        {
            List<Location> locations = await locationRepository.GetLocationsByUserIdAsync(userId);
            return locations.Select(Map).ToList();
        }

        public async Task<LocationResponseDto> GetLocationByIdAsync(Guid userId, Guid locationId)
        {
            Location location = await GetOwnedLocationAsync(userId, locationId);
            return Map(location);
        }

        public async Task<LocationResponseDto> UpdateLocationAsync(
            Guid userId, Guid locationId, UpdateLocationDto dto)
        {
            Location location = await GetOwnedLocationAsync(userId, locationId);

            if (IsEmpty(dto))
                throw new BadRequestException("At least one field must be provided");

            await ApplyChangesAsync(location, dto);

            return Map(await locationRepository.UpdateLocationAsync(location));
        }

        public async Task DeleteLocationAsync(Guid userId, Guid locationId)
        {
            Location location = await GetOwnedLocationAsync(userId, locationId);
            await locationRepository.DeleteLocationAsync(location);
        }

        private async Task<Location> GetOwnedLocationAsync(Guid userId, Guid locationId)
        {
            return await locationRepository.GetLocationByIdAsync(locationId, userId)
                ?? throw new NotFoundException("Location not found");
        }

        private static Location BuildLocation(Guid userId, CreateLocationDto dto) => new()
        {
            LocationId = Guid.NewGuid(),
            UserId = userId,
            Name = Normalize(dto.Name, nameof(dto.Name)),
            State = Normalize(dto.State, nameof(dto.State)),
            City = Normalize(dto.City, nameof(dto.City)),
            Neighborhood = Normalize(dto.Neighborhood, nameof(dto.Neighborhood)),
            Street = Normalize(dto.Street, nameof(dto.Street)),
            HouseNumber = Normalize(dto.HouseNumber, nameof(dto.HouseNumber))
        };

        private async Task ApplyChangesAsync(Location location, UpdateLocationDto dto)
        {
            if (dto.Name is not null)
                location.Name = await ValidateNameAsync(location, dto.Name);

            location.State = Apply(dto.State, location.State, nameof(dto.State));
            location.City = Apply(dto.City, location.City, nameof(dto.City));
            location.Neighborhood = Apply(dto.Neighborhood, location.Neighborhood, nameof(dto.Neighborhood));
            location.Street = Apply(dto.Street, location.Street, nameof(dto.Street));
            location.HouseNumber = Apply(dto.HouseNumber, location.HouseNumber, nameof(dto.HouseNumber));
        }

        private async Task<string> ValidateNameAsync(Location location, string value)
        {
            string name = Normalize(value, nameof(UpdateLocationDto.Name));

            if (await locationRepository.NameExistsAsync(location.UserId, name, location.LocationId))
                throw new ConflictException("A location with this name already exists");

            return name;
        }

        private static string Apply(string? value, string current, string fieldName) =>
            value is null ? current : Normalize(value, fieldName);

        private static bool IsEmpty(UpdateLocationDto dto) =>
            dto.Name is null &&
            dto.State is null &&
            dto.City is null &&
            dto.Neighborhood is null &&
            dto.Street is null &&
            dto.HouseNumber is null;

        private static string Normalize(string value, string fieldName)
        {
            string trimmed = value.Trim();

            if (trimmed.Length == 0)
                throw new BadRequestException($"{fieldName} cannot be empty");

            return trimmed;
        }

        private static LocationResponseDto Map(Location location) => new()
        {
            LocationId = location.LocationId,
            Name = location.Name,
            State = location.State,
            City = location.City,
            Neighborhood = location.Neighborhood,
            Street = location.Street,
            HouseNumber = location.HouseNumber,
            UserId = location.UserId
        };
    }
}
