
using DhakaRent.DataAccess.Context;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DhakaRent.DataAccess.Repositories
{
    public class BuildingRepository : IBuildingRepository
    {
        private readonly ApplicationDbContext _context;

        public BuildingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all buildings belonging to an owner
        public async Task<IEnumerable<Building>> GetByOwnerIdAsync(
            string ownerId)
        {
            return await _context.Buildings
                .AsNoTracking()
                .Where(b => b.OwnerId == ownerId)
                .OrderByDescending(b => b.CreatedAtUtc)
                .ThenByDescending(b => b.Id)
                .ToListAsync();
        }

        // Get a building by ID and verify its owner
        public async Task<Building?> GetByIdAndOwnerAsync(
            int buildingId,
            string ownerId)
        {
            return await _context.Buildings
                .FirstOrDefaultAsync(b =>
                    b.Id == buildingId &&
                    b.OwnerId == ownerId);
        }

        // Add a new building
        public async Task AddAsync(Building building)
        {
            await _context.Buildings.AddAsync(building);
        }

        // Update an existing building
        public void Update(Building building)
        {
            _context.Buildings.Update(building);
        }

        // Save database changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
