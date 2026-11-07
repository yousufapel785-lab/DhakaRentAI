
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
    }
}
