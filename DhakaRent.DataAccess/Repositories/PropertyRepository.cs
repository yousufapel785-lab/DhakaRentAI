
using DhakaRent.DataAccess.Context;
using DhakaRent.Entities.Models;
using Microsoft.EntityFrameworkCore;
using DhakaRent.Facade.Interfaces;

namespace DhakaRent.DataAccess.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly ApplicationDbContext _context;

        public PropertyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieve a property by its ID
        public async Task<Property?> GetByIdAsync(int id)
        {
            return await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Retrieve all properties
        public async Task<IEnumerable<Property>> GetAllAsync()
        {
            return await _context.Properties
                .AsNoTracking()
                .ToListAsync();
        }

        // Retrieve properties belonging to a specific owner
        public async Task<IEnumerable<Property>> GetByOwnerIdAsync(
            string ownerId)
        {
            return await _context.Properties
                .AsNoTracking()
                .Where(p => p.OwnerId == ownerId)
                .ToListAsync();
        }

        // Add a new property
        public async Task AddAsync(Property property)
        {
            await _context.Properties.AddAsync(property);
        }

        // Update an existing property
        public void Update(Property property)
        {
            _context.Properties.Update(property);
        }

        // Delete a property
        public void Delete(Property property)
        {
            _context.Properties.Remove(property);
        }

        // Save database changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
