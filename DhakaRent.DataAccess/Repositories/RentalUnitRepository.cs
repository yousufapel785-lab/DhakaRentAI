
using DhakaRent.DataAccess.Context;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DhakaRent.DataAccess.Repositories
{
    public class RentalUnitRepository : IRentalUnitRepository
    {
        private readonly ApplicationDbContext _context;

        public RentalUnitRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all units of an owner's floor
        public async Task<IEnumerable<RentalUnit>> GetByFloorIdAsync(
            int floorId,
            string ownerId)
        {
            return await _context.RentalUnits
                .AsNoTracking()
                .Where(u =>
                    u.FloorId == floorId &&
                    u.Floor.Building.OwnerId == ownerId)
                .OrderBy(u => u.UnitNumber)
                .ThenBy(u => u.Id)
                .ToListAsync();
        }

        // Get one unit with ownership verification
        public async Task<RentalUnit?> GetByIdAndOwnerAsync(
            int unitId,
            string ownerId)
        {
            return await _context.RentalUnits
                .FirstOrDefaultAsync(u =>
                    u.Id == unitId &&
                    u.Floor.Building.OwnerId == ownerId);
        }

        // Check duplicate unit number on the same floor
        public async Task<bool> UnitNumberExistsAsync(
            int floorId,
            string unitNumber,
            int? excludeUnitId = null)
        {
            return await _context.RentalUnits
                .AsNoTracking()
                .AnyAsync(u =>
                    u.FloorId == floorId &&
                    u.UnitNumber == unitNumber &&
                    (!excludeUnitId.HasValue ||
                     u.Id != excludeUnitId.Value));
        }

        // Add a new rental unit
        public async Task AddAsync(RentalUnit unit)
        {
            await _context.RentalUnits.AddAsync(unit);
        }

        // Update an existing rental unit
        public void Update(RentalUnit unit)
        {
            _context.RentalUnits.Update(unit);
        }

        // Save changes to database
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
