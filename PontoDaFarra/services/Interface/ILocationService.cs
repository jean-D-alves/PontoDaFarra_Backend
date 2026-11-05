using PontoDaFarra.DTOs.Location;

namespace PontoDaFarra.services.Interface
{
    public interface ILocationService
    {
        Task<LocationResponseDto> CreateLocationAsync(Guid userId, CreateLocationDto dto);
        Task<List<LocationResponseDto>> GetLocationsAsync(Guid userId);
        Task<LocationResponseDto> GetLocationByIdAsync(Guid userId, Guid locationId);
        Task<LocationResponseDto> UpdateLocationAsync(Guid userId, Guid locationId, UpdateLocationDto dto);
        Task DeleteLocationAsync(Guid userId, Guid locationId);
    }
}
