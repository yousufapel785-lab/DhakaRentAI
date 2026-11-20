
using DhakaRent.DataAccess.Context;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DhakaRent.DataAccess.Repositories
{
    public class FloorRepository : IFloorRepository
    {
        private readonly ApplicationDbContext _context;

        public FloorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get floors belonging to an owner's building
        public async Task<IEnumerable<Floor>> GetByBuildingIdAsync(
            int buildingId,
            string ownerId)
        {
            return await _context.Floors
                .AsNoTracking()
                .Where(f =>
                    f.BuildingId == buildingId &&
                    f.Building.OwnerId == ownerId)
                .OrderBy(f => f.FloorNumber)
                .ThenBy(f => f.Id)
                .ToListAsync();
        }

        // Get a floor only if its building belongs to the owner
        public async Task<Floor?> GetByIdAndOwnerAsync(
            int floorId,
            string ownerId)
        {
            return await _context.Floors
                .FirstOrDefaultAsync(f =>
                    f.Id == floorId &&
                    f.Building.OwnerId == ownerId);
        }

        // Check whether a floor number already exists
        public async Task<bool> FloorNumberExistsAsync(
            int buildingId,
            int floorNumber,
            int? excludeFloorId = null)
        {
            return await _context.Floors
                .AsNoTracking()
                .AnyAsync(f =>
                    f.BuildingId == buildingId &&
                    f.FloorNumber == floorNumber &&
                    (!excludeFloorId.HasValue ||
                     f.Id != excludeFloorId.Value));
        }

        // Add new floor
        public async Task AddAsync(Floor floor)
        {
            await _context.Floors.AddAsync(floor);
        }

        // Update existing floor
        public void Update(Floor floor)
        {
            _context.Floors.Update(floor);
        }

        // Save database changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
