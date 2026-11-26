
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface ISavedPropertyFacade
    {
        // Existing: Get saved property records
        Task<IReadOnlyList<SavedProperty>> GetMineAsync(
            string tenantId);

        // NEW: Get wishlist cards with property details
        Task<IReadOnlyList<SavedPropertyCardData>> GetMineCardsAsync(
            string tenantId);

        // Check whether a property is saved
        Task<bool> IsSavedAsync(
            string tenantId,
            int propertyId);

        // Save approved property to wishlist
        Task<bool> SaveAsync(
            string tenantId,
            int propertyId);

        // Remove property from wishlist
        Task<bool> RemoveAsync(
            string tenantId,
            int propertyId);
    }
}
