
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IBuildingRepository
    {
        // Get all buildings belonging to an owner
        Task<IEnumerable<Building>> GetByOwnerIdAsync(
            string ownerId);

        // Get a specific building only if it belongs to the owner
        Task<Building?> GetByIdAndOwnerAsync(
            int buildingId,
            string ownerId);

        // Add new building
        Task AddAsync(Building building);

        // Update existing building
        void Update(Building building);

        // Save changes to database
        Task SaveChangesAsync();
    }
}
