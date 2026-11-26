
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface ISavedPropertyRepository
    {
        // Get all saved records of a tenant
        Task<IReadOnlyList<SavedProperty>> GetByTenantIdAsync(
            string tenantId);

        // NEW: Get saved properties with details and cover image
        Task<IReadOnlyList<SavedPropertyCardData>> GetCardsByTenantIdAsync(
            string tenantId);

        // Check if a property is already saved
        Task<bool> ExistsAsync(
            string tenantId,
            int propertyId);

        // Get a tenant's specific saved property
        Task<SavedProperty?> GetByTenantAndPropertyAsync(
            string tenantId,
            int propertyId);

        // Add property to wishlist
        Task AddAsync(SavedProperty savedProperty);

        // Remove property from wishlist
        void Remove(SavedProperty savedProperty);

        // Save database changes
        Task SaveChangesAsync();
    }
}
