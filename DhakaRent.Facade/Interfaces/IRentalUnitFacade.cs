
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    // Data required to create or edit a rental unit
    public record RentalUnitInput(
        string UnitNumber,
        decimal SizeInSqFt,
        decimal StandardMonthlyRent,
        int Bedrooms,
        int Bathrooms
    );

    public interface IRentalUnitFacade
    {
        // Get units belonging to an owner's floor
        Task<IReadOnlyList<RentalUnit>> GetByFloorAsync(
            int floorId,
            string ownerId);

        // Get a unit with ownership verification
        Task<RentalUnit?> GetMineByIdAsync(
            int unitId,
            string ownerId);

        // Create a unit under an active building's floor
        Task<int> CreateAsync(
            int floorId,
            string ownerId,
            RentalUnitInput input);

        // Update an existing rental unit
        Task<bool> UpdateAsync(
            int unitId,
            string ownerId,
            RentalUnitInput input);
    }
}
