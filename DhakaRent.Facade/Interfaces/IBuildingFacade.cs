
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    // Data required to create or edit a building
    public record BuildingInput(
        string Name,
        string Address,
        string Area,
        decimal? Latitude,
        decimal? Longitude
    );

    public interface IBuildingFacade
    {
        // Get buildings belonging to the logged-in owner
        Task<IReadOnlyList<Building>> GetMineAsync(
            string ownerId);

        // Get one building with ownership verification
        Task<Building?> GetMineByIdAsync(
            int buildingId,
            string ownerId);

        // Create a new building
        Task<int> CreateAsync(
            string ownerId,
            BuildingInput input);

        // Edit an existing building
        Task<bool> UpdateAsync(
            int buildingId,
            string ownerId,
            BuildingInput input);

        // Archive a building (soft delete)
        Task<bool> ArchiveAsync(
            int buildingId,
            string ownerId);
    }
}
