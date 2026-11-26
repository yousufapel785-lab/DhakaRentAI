
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IViewingRequestFacade
    {
        // 1. Get all viewing requests submitted by a tenant.
        Task<IReadOnlyList<ViewingRequest>> GetMineAsync(
            string tenantId);

        // 2. Get all viewing requests received by an owner.
        Task<IReadOnlyList<ViewingRequest>> GetReceivedAsync(
            string ownerId);

        // Retrieve property details for viewing request lists.
        // Use only IDs from the logged-in user's
        // authorized viewing requests.
        Task<IReadOnlyDictionary<int, Property>>
            GetPropertiesByIdsAsync(
                IReadOnlyCollection<int> propertyIds);

        // 3. Get a viewing request belonging to a tenant.
        Task<ViewingRequest?> GetMineByIdAsync(
            int requestId,
            string tenantId);

        // 4. Get a viewing request for an owner's property.
        Task<ViewingRequest?> GetReceivedByIdAsync(
            int requestId,
            string ownerId);

        // 5. Tenant creates a viewing request.
        Task<int> CreateAsync(
            int propertyId,
            string tenantId,
            DateTime requestedDateTimeUtc,
            string? tenantMessage);

        // 6. Owner accepts the requested viewing time.
        Task AcceptAsync(
            int requestId,
            string ownerId,
            string? ownerResponse);

        // 7. Owner rejects a viewing request.
        Task RejectAsync(
            int requestId,
            string ownerId,
            string? ownerResponse);

        // 8. Owner proposes an alternative viewing time.
        Task ProposeRescheduleAsync(
            int requestId,
            string ownerId,
            DateTime proposedDateTimeUtc,
            string? ownerResponse);

        // 9. Tenant accepts the owner's alternative time.
        Task AcceptProposedTimeAsync(
            int requestId,
            string tenantId);

        // 10. Tenant cancels a viewing request.
        Task CancelAsync(
            int requestId,
            string tenantId);

        // 11. Owner marks an accepted viewing as completed.
        Task CompleteAsync(
            int requestId,
            string ownerId);
    }
}
