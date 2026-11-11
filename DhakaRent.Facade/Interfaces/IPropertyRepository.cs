
using DhakaRent.Entities.Enums;
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IPropertyRepository
    {
        Task<Property?> GetByIdAsync(int id);

        Task<IEnumerable<Property>> GetAllAsync();

        Task<IEnumerable<Property>> GetByOwnerIdAsync(string ownerId);
        Task<IEnumerable<Property>> GetByStatusAsync(
            ListingStatus status);
        Task AddAsync(Property property);

        void Update(Property property);

        void Delete(Property property);

        Task SaveChangesAsync();

        Task<IEnumerable<Property>> SearchApprovedPropertiesAsync(
            string? area,
            PropertyType? propertyType,
            decimal? minRent,
            decimal? maxRent,
            int? bedrooms);

    }
}
