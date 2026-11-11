using DhakaRent.Entities.Models;
using DhakaRent.Entities.Enums;

namespace DhakaRent.Facade.Interfaces
{
    public interface IPropertyFacade
    {
        Task<Property?> GetPropertyByIdAsync(int id);

        Task<IEnumerable<Property>> GetAllPropertiesAsync();

        Task<IEnumerable<Property>> GetPropertiesByOwnerIdAsync(
            string ownerId);

        Task<IEnumerable<Property>> GetPropertiesByStatusAsync(
            ListingStatus status);

        Task CreatePropertyAsync(Property property);

        Task UpdatePropertyAsync(Property property);

        Task DeletePropertyAsync(int id);
        Task<bool> ApprovePropertyAsync(int id);

        Task<bool> RejectPropertyAsync(int id, string reason);


        Task<IEnumerable<Property>> SearchApprovedPropertiesAsync(
            string? area,
            PropertyType? propertyType,
            decimal? minRent,
            decimal? maxRent,
            int? bedrooms);

    }
}