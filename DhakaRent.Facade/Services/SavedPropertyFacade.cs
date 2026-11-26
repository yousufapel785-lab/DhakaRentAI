
using DhakaRent.Entities.Enums;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;

namespace DhakaRent.Facade.Services
{
    public class SavedPropertyFacade : ISavedPropertyFacade
    {
        private readonly ISavedPropertyRepository _savedRepository;
        private readonly IPropertyRepository _propertyRepository;

        public SavedPropertyFacade(
            ISavedPropertyRepository savedRepository,
            IPropertyRepository propertyRepository)
        {
            _savedRepository = savedRepository;
            _propertyRepository = propertyRepository;
        }

        // 1. Get the tenant's saved, publicly approved properties
        public async Task<IReadOnlyList<SavedProperty>> GetMineAsync(
            string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return new List<SavedProperty>();
            }

            var savedRecords = await _savedRepository
                .GetByTenantIdAsync(tenantId);

            var approvedSavedProperties = new List<SavedProperty>();

            foreach (var saved in savedRecords)
            {
                var property = await _propertyRepository
                    .GetByIdAsync(saved.PropertyId);

                if (property != null &&
                    property.ListingStatus == ListingStatus.Approved)
                {
                    approvedSavedProperties.Add(saved);
                }
            }

            return approvedSavedProperties;
        }


        // Get saved property cards with full details
        public async Task<IReadOnlyList<SavedPropertyCardData>>
            GetMineCardsAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return new List<SavedPropertyCardData>();
            }

            return await _savedRepository
                .GetCardsByTenantIdAsync(tenantId);
        }


        // 2. Check whether the tenant saved a property
        public async Task<bool> IsSavedAsync(
            string tenantId,
            int propertyId)
        {
            if (string.IsNullOrWhiteSpace(tenantId) ||
                propertyId <= 0)
            {
                return false;
            }

            return await _savedRepository.ExistsAsync(
                tenantId,
                propertyId);
        }

        // 3. Save an approved property to the wishlist
        public async Task<bool> SaveAsync(
            string tenantId,
            int propertyId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException(
                    "Tenant information is required.");
            }

            if (propertyId <= 0)
            {
                throw new ArgumentException(
                    "Invalid property ID.");
            }

            // Step 1: Verify property and approval status
            var property = await _propertyRepository
                .GetByIdAsync(propertyId);

            if (property == null ||
                property.ListingStatus != ListingStatus.Approved)
            {
                throw new ArgumentException(
                    "Only approved properties can be saved.");
            }

            // Step 2: Prevent duplicate saves
            bool alreadySaved = await _savedRepository
                .ExistsAsync(tenantId, propertyId);

            if (alreadySaved)
            {
                return false;
            }

            // Step 3: Create saved property record
            var savedProperty = new SavedProperty
            {
                TenantId = tenantId,
                PropertyId = propertyId,
                SavedAt = DateTime.UtcNow
            };

            // Step 4: Save to database
            await _savedRepository.AddAsync(savedProperty);
            await _savedRepository.SaveChangesAsync();

            return true;
        }

        // 4. Remove a property from the tenant's wishlist
        public async Task<bool> RemoveAsync(
            string tenantId,
            int propertyId)
        {
            if (string.IsNullOrWhiteSpace(tenantId) ||
                propertyId <= 0)
            {
                return false;
            }

            // Find only this tenant's saved record
            var savedProperty = await _savedRepository
                .GetByTenantAndPropertyAsync(
                    tenantId,
                    propertyId);

            if (savedProperty == null)
            {
                return false;
            }

            // Remove saved record
            _savedRepository.Remove(savedProperty);
            await _savedRepository.SaveChangesAsync();

            return true;
        }
    }
}
