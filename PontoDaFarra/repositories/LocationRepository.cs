using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PontoDaFarra.context;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;

namespace PontoDaFarra.repositories
{
    public class LocationRepository(AppDbContext context) : ILocationRepository
    {
        public async Task<Location> CreateLocationAsync(Location location)
        {
            context.Locations.Add(location);
            await context.SaveChangesAsync();
            return location;
        }

        public async Task<Location?> GetLocationByIdAsync(Guid locationId, Guid userId)
        {
            return await context.Locations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LocationId == locationId && l.UserId == userId);
        }

        public async Task<List<Location>> GetLocationsByUserIdAsync(Guid userId)
        {
            return await context.Locations
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
        {
            return await context.Locations.CountAsync(l => l.UserId == userId);
        }

        public async Task<bool> NameExistsAsync(Guid userId, string name, Guid? excludeLocationId = null)
        {
            IQueryable<Location> query = context.Locations
                .AsNoTracking()
                .Where(l => l.UserId == userId && l.Name == name);

            if (excludeLocationId.HasValue)
                query = query.Where(l => l.LocationId != excludeLocationId.Value);

            return await query.AnyAsync();
        }

        public async Task<Location> UpdateLocationAsync(Location location)
        {
            EntityEntry<Location> entry = context.Locations.Update(location);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task DeleteLocationAsync(Location location)
        {
            context.Locations.Remove(location);
            await context.SaveChangesAsync();
        }
    }
}
