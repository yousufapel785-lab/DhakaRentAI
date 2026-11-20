
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IRentalUnitRepository
    {
        // Get units of an owner's floor
        Task<IEnumerable<RentalUnit>> GetByFloorIdAsync(
            int floorId,
            string ownerId);

        // Get one unit with ownership verification
        Task<RentalUnit?> GetByIdAndOwnerAsync(
            int unitId,
            string ownerId);

        // Check duplicate unit number in same floor
        Task<bool> UnitNumberExistsAsync(
            int floorId,
            string unitNumber,
            int? excludeUnitId = null);

        // Add new rental unit
        Task AddAsync(RentalUnit unit);

        // Update existing rental unit
        void Update(RentalUnit unit);

        // Save database changes
        Task SaveChangesAsync();
    }
}
