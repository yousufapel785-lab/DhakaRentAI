
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IPropertyFacade
    {
        // Retrieve a property by ID
        Task<Property?> GetPropertyByIdAsync(int id);

        // Retrieve all properties
        Task<IEnumerable<Property>> GetAllPropertiesAsync();

        // Retrieve properties belonging to an owner
        Task<IEnumerable<Property>> GetPropertiesByOwnerIdAsync(
            string ownerId);

        // Create a new property
        Task CreatePropertyAsync(Property property);

        // Update an existing property
        Task UpdatePropertyAsync(Property property);

        // Delete a property
        Task DeletePropertyAsync(int id);
    }
}
