
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IViewingRequestRepository
    {
        // 1. Get all viewing requests
        // submitted by a specific tenant.
        Task<IReadOnlyList<ViewingRequest>>
            GetByTenantIdAsync(string tenantId);

        // 2. Get viewing requests received
        // for properties owned by a specific owner.
        Task<IReadOnlyList<ViewingRequest>>
            GetByOwnerIdAsync(string ownerId);

        // Retrieve property details for viewing request lists.
        // Fetch multiple properties in a single database query.
        Task<IReadOnlyDictionary<int, Property>> GetPropertiesByIdsAsync(
            IReadOnlyCollection<int> propertyIds);


        // 3. Get a viewing request only if
        // it belongs to the specified tenant.
        Task<ViewingRequest?> GetByIdAndTenantAsync(
            int requestId,
            string tenantId);

        // 4. Get a viewing request only if
        // its property belongs to the specified owner.
        Task<ViewingRequest?> GetByIdAndOwnerAsync(
            int requestId,
            string ownerId);

        // 5. Add a new viewing request.
        Task AddAsync(ViewingRequest request);

        // 6. Update an existing viewing request.
        void Update(ViewingRequest request);

        // 7. Save database changes.
        Task SaveChangesAsync();
    }
}
