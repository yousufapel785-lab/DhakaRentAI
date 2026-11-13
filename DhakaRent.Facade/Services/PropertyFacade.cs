
using DhakaRent.Entities.Enums;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;

namespace DhakaRent.Facade.Services
{
    public class PropertyFacade : IPropertyFacade
    {
        private readonly IPropertyRepository _propertyRepository;

        public PropertyFacade(
            IPropertyRepository propertyRepository)
        {
            _propertyRepository = propertyRepository;
        }

        public async Task<Property?> GetPropertyByIdAsync(int id)
        {
            return await _propertyRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
        {
            return await _propertyRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Property>> GetPropertiesByOwnerIdAsync(
            string ownerId)
        {
            return await _propertyRepository.GetByOwnerIdAsync(ownerId);
        }

        public async Task CreatePropertyAsync(Property property)
        {
            property.CreatedAt = DateTime.UtcNow;
            property.ListingStatus = DhakaRent.Entities.Enums.ListingStatus.Pending;

            await _propertyRepository.AddAsync(property);
            await _propertyRepository.SaveChangesAsync();
        }

        public async Task UpdatePropertyAsync(Property property)
        {
            property.UpdatedAt = DateTime.UtcNow;

            _propertyRepository.Update(property);
            await _propertyRepository.SaveChangesAsync();
        }

        public async Task DeletePropertyAsync(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);

            if (property == null)
            {
                return;
            }

            _propertyRepository.Delete(property);
            await _propertyRepository.SaveChangesAsync();
        }
        public async Task<IEnumerable<Property>> GetPropertiesByStatusAsync(
    ListingStatus status)
        {
            return await _propertyRepository.GetByStatusAsync(status);
        }
        public async Task<bool> ApprovePropertyAsync(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);

            if (property == null ||
                property.ListingStatus != ListingStatus.Pending)
            {
                return false;
            }

            property.ListingStatus = ListingStatus.Approved;
            property.RejectionReason = null;
            property.UpdatedAt = DateTime.UtcNow;

            _propertyRepository.Update(property);

            await _propertyRepository.SaveChangesAsync();

            return true;
        }
        public async Task<bool> RejectPropertyAsync(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return false;
            }

            var property = await _propertyRepository.GetByIdAsync(id);

            if (property == null ||
                property.ListingStatus != ListingStatus.Pending)
            {
                return false;
            }

            property.ListingStatus = ListingStatus.Rejected;
            property.RejectionReason = reason.Trim();
            property.UpdatedAt = DateTime.UtcNow;

            _propertyRepository.Update(property);

            await _propertyRepository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Property>> SearchApprovedPropertiesAsync(
            string? area,
            PropertyType? propertyType,
            decimal? minRent,
            decimal? maxRent,
            int? bedrooms)
        {
            // Validate the rent range
            if (minRent.HasValue && minRent.Value < 0)
                throw new ArgumentException("Minimum rent cannot be negative.");

            if (maxRent.HasValue && maxRent.Value < 0)
                throw new ArgumentException("Maximum rent cannot be negative.");

            if (minRent.HasValue && maxRent.HasValue &&
                minRent.Value > maxRent.Value)
                throw new ArgumentException(
                    "Minimum rent cannot be greater than maximum rent.");

            if (bedrooms.HasValue && bedrooms.Value < 0)
                throw new ArgumentException("Bedrooms cannot be negative.");

            // Delegate database filtering to Repository
            return await _propertyRepository.SearchApprovedPropertiesAsync(
                area,
                propertyType,
                minRent,
                maxRent,
                bedrooms);
        }

        public async Task<bool> UpdateOwnerPropertyAsync(
            int propertyId,
            string ownerId,
            Property updatedData)
        {
            // Step 1: Validate basic parameters
            if (propertyId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId) ||
                updatedData == null)
            {
                return false;
            }

            // Step 2: Load existing property
            var existingProperty = await _propertyRepository
                .GetByIdAsync(propertyId);

            if (existingProperty == null)
            {
                return false;
            }

            // Step 3: Verify ownership
            if (existingProperty.OwnerId != ownerId)
            {
                return false;
            }

            // Step 4: Check allowed listing statuses
            if (existingProperty.ListingStatus != ListingStatus.Pending &&
                existingProperty.ListingStatus != ListingStatus.Approved &&
                existingProperty.ListingStatus != ListingStatus.Rejected)
            {
                return false;
            }

            // Step 5: Validate important business fields
            if (string.IsNullOrWhiteSpace(updatedData.Title) ||
                string.IsNullOrWhiteSpace(updatedData.Area) ||
                string.IsNullOrWhiteSpace(updatedData.Address) ||
                !Enum.IsDefined(updatedData.PropertyType) ||
                updatedData.SizeInSqFt <= 0 ||
                updatedData.MonthlyRent <= 0 ||
                updatedData.Bedrooms < 0 ||
                updatedData.Bathrooms < 0)
            {
                return false;
            }

            // Step 6: Update allowed property fields only
            existingProperty.Title = updatedData.Title.Trim();
            existingProperty.PropertyType = updatedData.PropertyType;
            existingProperty.Description =
                updatedData.Description?.Trim() ?? string.Empty;

            existingProperty.Area = updatedData.Area.Trim();
            existingProperty.Address = updatedData.Address.Trim();
            existingProperty.SizeInSqFt = updatedData.SizeInSqFt;
            existingProperty.Bedrooms = updatedData.Bedrooms;
            existingProperty.Bathrooms = updatedData.Bathrooms;
            existingProperty.IsFurnished = updatedData.IsFurnished;
            existingProperty.MonthlyRent = updatedData.MonthlyRent;
            existingProperty.AvailableFrom = updatedData.AvailableFrom;

            // Step 7: Require admin approval again
            existingProperty.ListingStatus = ListingStatus.Pending;
            existingProperty.RejectionReason = null;
            existingProperty.UpdatedAt = DateTime.UtcNow;

            // Step 8: Save changes
            _propertyRepository.Update(existingProperty);
            await _propertyRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateOwnerPropertyAsync(
            int propertyId,
            string ownerId)
        {
            // Step 1: Validate input
            if (propertyId <= 0 || string.IsNullOrWhiteSpace(ownerId))
            {
                return false;
            }

            // Step 2: Get existing property from database
            var property = await _propertyRepository.GetByIdAsync(propertyId);

            if (property == null)
            {
                return false;
            }

            // Step 3: Verify property ownership
            if (property.OwnerId != ownerId)
            {
                return false;
            }

            // Step 4: Check allowed statuses
            if (property.ListingStatus != ListingStatus.Pending &&
                property.ListingStatus != ListingStatus.Approved &&
                property.ListingStatus != ListingStatus.Rejected)
            {
                return false;
            }

            // Step 5: Deactivate property
            property.ListingStatus = ListingStatus.Inactive;
            property.UpdatedAt = DateTime.UtcNow;

            // Step 6: Save changes
            _propertyRepository.Update(property);
            await _propertyRepository.SaveChangesAsync();

            return true;
        }



    }
}
