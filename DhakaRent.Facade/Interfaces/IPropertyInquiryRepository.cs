
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IPropertyInquiryRepository
    {
        // Get inquiries submitted by a tenant
        Task<IReadOnlyList<PropertyInquiry>> GetByTenantIdAsync(
            string tenantId);

        // Get inquiries received for an owner's properties
        Task<IReadOnlyList<PropertyInquiry>> GetByOwnerIdAsync(
            string ownerId);

        // Get one inquiry belonging to a tenant
        Task<PropertyInquiry?> GetByIdAndTenantAsync(
            int inquiryId,
            string tenantId);

        // Get one inquiry belonging to a property owner
        Task<PropertyInquiry?> GetByIdAndOwnerAsync(
            int inquiryId,
            string ownerId);

        // Add a new inquiry
        Task AddAsync(PropertyInquiry inquiry);

        // Update inquiry status
        void Update(PropertyInquiry inquiry);

        // Save database changes
        Task SaveChangesAsync();
    }
}
