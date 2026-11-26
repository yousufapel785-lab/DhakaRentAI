
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IPropertyInquiryFacade
    {
        // 1. Get inquiries sent by a tenant
        Task<IReadOnlyList<PropertyInquiry>> GetMineAsync(
            string tenantId);

        // 2. Get inquiries received by a property owner
        Task<IReadOnlyList<PropertyInquiry>> GetReceivedAsync(
            string ownerId);

        // 3. Get one inquiry belonging to a tenant
        Task<PropertyInquiry?> GetMineByIdAsync(
            int inquiryId,
            string tenantId);

        // 4. Get one inquiry received by an owner
        Task<PropertyInquiry?> GetReceivedByIdAsync(
            int inquiryId,
            string ownerId);

        // 5. Send a new inquiry about a property
        Task<int> CreateAsync(
            int propertyId,
            string tenantId,
            string initialMessage);

        // Retrieve property information for inquiry lists.
        // IDs must come from the current user's
        // authorized inquiries.
        Task<IReadOnlyDictionary<int, Property>>
            GetPropertiesByIdsAsync(
                IReadOnlyCollection<int> propertyIds);

    }
}
