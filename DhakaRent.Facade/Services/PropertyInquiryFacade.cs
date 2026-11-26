
using DhakaRent.Entities.Enums;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;

namespace DhakaRent.Facade.Services
{
    public class PropertyInquiryFacade : IPropertyInquiryFacade
    {
        private readonly IPropertyInquiryRepository _inquiryRepository;
        private readonly IPropertyRepository _propertyRepository;

        public PropertyInquiryFacade(
            IPropertyInquiryRepository inquiryRepository,
            IPropertyRepository propertyRepository)
        {
            _inquiryRepository = inquiryRepository;
            _propertyRepository = propertyRepository;
        }

        // 1. Get inquiries sent by a tenant
        public async Task<IReadOnlyList<PropertyInquiry>> GetMineAsync(
            string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return new List<PropertyInquiry>();
            }

            return await _inquiryRepository
                .GetByTenantIdAsync(tenantId);
        }

        // 2. Get inquiries received by a property owner
        public async Task<IReadOnlyList<PropertyInquiry>> GetReceivedAsync(
            string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return new List<PropertyInquiry>();
            }

            return await _inquiryRepository
                .GetByOwnerIdAsync(ownerId);
        }

        // 3. Get a specific inquiry belonging to a tenant
        public async Task<PropertyInquiry?> GetMineByIdAsync(
            int inquiryId,
            string tenantId)
        {
            if (inquiryId <= 0 ||
                string.IsNullOrWhiteSpace(tenantId))
            {
                return null;
            }

            return await _inquiryRepository
                .GetByIdAndTenantAsync(inquiryId, tenantId);
        }

        // 4. Get a specific inquiry received by a property owner
        public async Task<PropertyInquiry?> GetReceivedByIdAsync(
            int inquiryId,
            string ownerId)
        {
            if (inquiryId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return null;
            }

            return await _inquiryRepository
                .GetByIdAndOwnerAsync(inquiryId, ownerId);
        }

        // 5. Create a new property inquiry
        public async Task<int> CreateAsync(
            int propertyId,
            string tenantId,
            string initialMessage)
        {
            // Step 1: Validate tenant information
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException(
                    "Tenant information is required.");
            }

            // Step 2: Validate property ID
            if (propertyId <= 0)
            {
                throw new ArgumentException(
                    "Invalid property ID.");
            }

            // Step 3: Validate message
            if (string.IsNullOrWhiteSpace(initialMessage))
            {
                throw new ArgumentException(
                    "Inquiry message is required.");
            }

            string message = initialMessage.Trim();

            if (message.Length > 2000)
            {
                throw new ArgumentException(
                    "Inquiry message cannot exceed 2000 characters.");
            }

            // Step 4: Find the property
            var property = await _propertyRepository
                .GetByIdAsync(propertyId);

            if (property == null)
            {
                throw new ArgumentException(
                    "Property not found.");
            }

            // Step 5: Only approved properties allow inquiries
            if (property.ListingStatus != ListingStatus.Approved)
            {
                throw new ArgumentException(
                    "Inquiries are allowed only for approved properties.");
            }

            // Step 6: Prevent inquiries about own property
            if (property.OwnerId == tenantId)
            {
                throw new ArgumentException(
                    "You cannot send an inquiry about your own property.");
            }

            // Step 7: Create new inquiry
            var inquiry = new PropertyInquiry
            {
                PropertyId = propertyId,
                TenantId = tenantId,
                InitialMessage = message,
                Status = InquiryStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Step 8: Save to database
            await _inquiryRepository.AddAsync(inquiry);
            await _inquiryRepository.SaveChangesAsync();

            return inquiry.Id;
        }

        // Retrieve property information for inquiry lists.
        // The caller must supply property IDs obtained
        // from the logged-in user's authorized inquiries.
        public Task<IReadOnlyDictionary<int, Property>>
            GetPropertiesByIdsAsync(
                IReadOnlyCollection<int> propertyIds)
        {
            // Step A: Validate input.
            ArgumentNullException.ThrowIfNull(propertyIds);

            // Step B: Retrieve properties through
            // the existing Property Repository.
            return _propertyRepository
                .GetByIdsAsync(propertyIds);
        }

    }
}
