
using DhakaRent.DataAccess.Context;
using DhakaRent.Entities.Enums;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IEnumerable<Property>> GetByStatusAsync(
            ListingStatus status)
        {
            return await _context.Properties
                .AsNoTracking()
                .Where(p => p.ListingStatus == status)
                .OrderBy(p => p.CreatedAt)
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

        public async Task<IEnumerable<Property>> SearchApprovedPropertiesAsync(
            string? area,
            PropertyType? propertyType,
            decimal? minRent,
            decimal? maxRent,
            int? bedrooms)
        {
            // Start with approved properties only
            var query = _context.Properties
                .AsNoTracking()
                .Where(p => p.ListingStatus == ListingStatus.Approved);

            // Filter by area
            if (!string.IsNullOrWhiteSpace(area))
            {
                var searchArea = area.Trim();

                query = query.Where(p =>
                    p.Area.Contains(searchArea));
            }

            // Filter by property type
            if (propertyType.HasValue)
            {
                query = query.Where(p =>
                    p.PropertyType == propertyType.Value);
            }

            // Filter by minimum rent
            if (minRent.HasValue)
            {
                query = query.Where(p =>
                    p.MonthlyRent >= minRent.Value);
            }

            // Filter by maximum rent
            if (maxRent.HasValue)
            {
                query = query.Where(p =>
                    p.MonthlyRent <= maxRent.Value);
            }

            // Filter by bedrooms
            if (bedrooms.HasValue)
            {
                query = query.Where(p =>
                    p.Bedrooms == bedrooms.Value);
            }

            // Execute query in SQL Server
            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

    }
}
