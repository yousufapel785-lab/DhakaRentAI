
using DhakaRent.DataAccess.Context;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DhakaRent.DataAccess.Repositories
{
    public class ViewingRequestRepository
        : IViewingRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ViewingRequestRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Get viewing requests submitted by a tenant.
        public async Task<IReadOnlyList<ViewingRequest>>
            GetByTenantIdAsync(string tenantId)
        {
            return await _context.ViewingRequests
                .AsNoTracking()
                .Where(v => v.TenantId == tenantId)
                .OrderByDescending(v => v.CreatedAt)
                .ThenByDescending(v => v.Id)
                .ToListAsync();
        }

        // 2. Get viewing requests for an owner's properties.
        public async Task<IReadOnlyList<ViewingRequest>>
            GetByOwnerIdAsync(string ownerId)
        {
            var query =
                from request in _context.ViewingRequests
                join property in _context.Properties
                    on request.PropertyId equals property.Id
                where property.OwnerId == ownerId
                orderby request.CreatedAt descending,
                        request.Id descending
                select request;

            return await query
                .AsNoTracking()
                .ToListAsync();
        }

        // Retrieve multiple properties by their IDs.
        // Used to display property names and locations
        // in Tenant and PropertyOwner viewing request lists.
        public async Task<IReadOnlyDictionary<int, Property>>
            GetPropertiesByIdsAsync(
                IReadOnlyCollection<int> propertyIds)
        {
            // Step A: Validate input.
            ArgumentNullException.ThrowIfNull(propertyIds);

            // Step B: Remove invalid and duplicate IDs.
            int[] ids = propertyIds
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            // Step C: Avoid unnecessary database queries.
            if (ids.Length == 0)
            {
                return new Dictionary<int, Property>();
            }

            // Step D: Fetch properties in one database query.
            // AsNoTracking improves read-only performance.
            var properties = await _context.Properties
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // Step E: Return properties indexed by ID.
            return properties;
        }

        // 3. Get a specific request belonging to a tenant.
        public async Task<ViewingRequest?>
            GetByIdAndTenantAsync(
                int requestId,
                string tenantId)
        {
            return await _context.ViewingRequests
                .FirstOrDefaultAsync(v =>
                    v.Id == requestId &&
                    v.TenantId == tenantId);
        }

        // 4. Get a specific request for an owner's property.
        public async Task<ViewingRequest?>
            GetByIdAndOwnerAsync(
                int requestId,
                string ownerId)
        {
            var query =
                from request in _context.ViewingRequests
                join property in _context.Properties
                    on request.PropertyId equals property.Id
                where request.Id == requestId &&
                      property.OwnerId == ownerId
                select request;

            return await query.FirstOrDefaultAsync();
        }

        // 5. Add a new viewing request.
        public async Task AddAsync(ViewingRequest request)
        {
            await _context.ViewingRequests
                .AddAsync(request);
        }

        // 6. Update an existing viewing request.
        public void Update(ViewingRequest request)
        {
            _context.ViewingRequests.Update(request);
        }

        // 7. Save database changes.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
