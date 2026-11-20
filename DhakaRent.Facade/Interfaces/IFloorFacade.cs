
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    // Data required to create or edit a floor
    public record FloorInput(
        int FloorNumber,
        string Label
    );

    public interface IFloorFacade
    {
        // Get floors of a building owned by the user
        Task<IReadOnlyList<Floor>> GetByBuildingAsync(
            int buildingId,
            string ownerId);

        // Get one floor with ownership verification
        Task<Floor?> GetMineByIdAsync(
            int floorId,
            string ownerId);

        // Create a floor in an owner's active building
        Task<int> CreateAsync(
            int buildingId,
            string ownerId,
            FloorInput input);

        // Update an existing floor
        Task<bool> UpdateAsync(
            int floorId,
            string ownerId,
            FloorInput input);
    }
}
