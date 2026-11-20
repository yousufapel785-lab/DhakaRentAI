
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IFloorRepository
    {
        // Get floors of a building owned by this user
        Task<IEnumerable<Floor>> GetByBuildingIdAsync(
            int buildingId,
            string ownerId);

        // Get one floor with ownership verification
        Task<Floor?> GetByIdAndOwnerAsync(
            int floorId,
            string ownerId);

        // Check duplicate floor number
        Task<bool> FloorNumberExistsAsync(
            int buildingId,
            int floorNumber,
            int? excludeFloorId = null);

        // Add new floor
        Task AddAsync(Floor floor);

        // Update existing floor
        void Update(Floor floor);

        // Save changes
        Task SaveChangesAsync();
    }
}
