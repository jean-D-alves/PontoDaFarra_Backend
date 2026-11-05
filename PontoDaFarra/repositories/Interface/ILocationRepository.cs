using PontoDaFarra.models;

namespace PontoDaFarra.repositories.Interface
{
    public interface ILocationRepository
    {
        Task<Location> CreateLocationAsync(Location location);
        Task<Location?> GetLocationByIdAsync(Guid locationId, Guid userId);
        Task<List<Location>> GetLocationsByUserIdAsync(Guid userId);
        Task<int> CountByUserIdAsync(Guid userId);
        Task<bool> NameExistsAsync(Guid userId, string name, Guid? excludeLocationId = null);
        Task<Location> UpdateLocationAsync(Location location);
        Task DeleteLocationAsync(Location location);
    }
}
