
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

    }
}
