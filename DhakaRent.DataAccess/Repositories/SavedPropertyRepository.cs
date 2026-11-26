
using DhakaRent.DataAccess.Context;
using DhakaRent.Entities.Enums;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DhakaRent.DataAccess.Repositories
{
    public class SavedPropertyRepository : ISavedPropertyRepository
    {
        private readonly ApplicationDbContext _context;

        public SavedPropertyRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Get all saved records of a tenant
        public async Task<IReadOnlyList<SavedProperty>> GetByTenantIdAsync(
            string tenantId)
        {
            return await _context.SavedProperties
                .AsNoTracking()
                .Where(sp => sp.TenantId == tenantId)
                .OrderByDescending(sp => sp.SavedAt)
                .ThenByDescending(sp => sp.Id)
                .ToListAsync();
        }

        // 2. NEW: Get saved property cards with property details
        // and cover image using a single database query
        public async Task<IReadOnlyList<SavedPropertyCardData>>
            GetCardsByTenantIdAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return new List<SavedPropertyCardData>();
            }

            var cards = await (
                from saved in _context.SavedProperties.AsNoTracking()

                join property in _context.Properties.AsNoTracking()
                    on saved.PropertyId equals property.Id

                where saved.TenantId == tenantId
                      && property.ListingStatus == ListingStatus.Approved

                orderby saved.SavedAt descending, saved.Id descending

                select new SavedPropertyCardData
                {
                    PropertyId = property.Id,

                    SavedAt = saved.SavedAt,

                    Title = property.Title,

                    Area = property.Area,

                    Address = property.Address,

                    MonthlyRent = property.MonthlyRent,

                    Bedrooms = property.Bedrooms,

                    Bathrooms = property.Bathrooms,

                    ImageUrl = _context.PropertyImages
                        .AsNoTracking()
                        .Where(image =>
                            image.PropertyId == property.Id)
                        .OrderByDescending(image => image.IsPrimary)
                        .ThenBy(image => image.DisplayOrder)
                        .ThenBy(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault()
                }
            ).ToListAsync();

            return cards;
        }

        // 3. Check if tenant already saved a property
        public async Task<bool> ExistsAsync(
            string tenantId,
            int propertyId)
        {
            return await _context.SavedProperties
                .AsNoTracking()
                .AnyAsync(sp =>
                    sp.TenantId == tenantId &&
                    sp.PropertyId == propertyId);
        }

        // 4. Get a specific saved property belonging to a tenant
        public async Task<SavedProperty?> GetByTenantAndPropertyAsync(
            string tenantId,
            int propertyId)
        {
            return await _context.SavedProperties
                .FirstOrDefaultAsync(sp =>
                    sp.TenantId == tenantId &&
                    sp.PropertyId == propertyId);
        }

        // 5. Add property to wishlist
        public async Task AddAsync(
            SavedProperty savedProperty)
        {
            await _context.SavedProperties
                .AddAsync(savedProperty);
        }

        // 6. Remove property from wishlist
        public void Remove(
            SavedProperty savedProperty)
        {
            _context.SavedProperties
                .Remove(savedProperty);
        }

        // 7. Save database changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
