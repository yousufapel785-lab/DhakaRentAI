
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IPropertyRepository
    {
        Task<Property?> GetByIdAsync(int id);

        Task<IEnumerable<Property>> GetAllAsync();

        Task<IEnumerable<Property>> GetByOwnerIdAsync(string ownerId);

        Task AddAsync(Property property);

        void Update(Property property);

        void Delete(Property property);

        Task SaveChangesAsync();
    }
}
