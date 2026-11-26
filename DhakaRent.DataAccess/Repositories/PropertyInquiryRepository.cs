
using DhakaRent.DataAccess.Context;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DhakaRent.DataAccess.Repositories
{
    public class PropertyInquiryRepository
        : IPropertyInquiryRepository
    {
        private readonly ApplicationDbContext _context;

        public PropertyInquiryRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Get inquiries submitted by a tenant
        public async Task<IReadOnlyList<PropertyInquiry>>
            GetByTenantIdAsync(string tenantId)
        {
            return await _context.PropertyInquiries
                .AsNoTracking()
                .Where(i => i.TenantId == tenantId)
                .OrderByDescending(i => i.CreatedAt)
                .ThenByDescending(i => i.Id)
                .ToListAsync();
        }

        // 2. Get inquiries received by a property owner
        public async Task<IReadOnlyList<PropertyInquiry>>
            GetByOwnerIdAsync(string ownerId)
        {
            return await (
                from inquiry in _context.PropertyInquiries
                    .AsNoTracking()

                join property in _context.Properties
                    .AsNoTracking()
                    on inquiry.PropertyId equals property.Id

                where property.OwnerId == ownerId

                orderby inquiry.CreatedAt descending,
                        inquiry.Id descending

                select inquiry

            ).ToListAsync();
        }

        // 3. Get a specific inquiry belonging to a tenant
        public async Task<PropertyInquiry?>
            GetByIdAndTenantAsync(
                int inquiryId,
                string tenantId)
        {
            return await _context.PropertyInquiries
                .FirstOrDefaultAsync(i =>
                    i.Id == inquiryId &&
                    i.TenantId == tenantId);
        }

        // 4. Get a specific inquiry belonging to a property owner
        public async Task<PropertyInquiry?>
            GetByIdAndOwnerAsync(
                int inquiryId,
                string ownerId)
        {
            return await (
                from inquiry in _context.PropertyInquiries

                join property in _context.Properties
                    on inquiry.PropertyId equals property.Id

                where inquiry.Id == inquiryId
                      && property.OwnerId == ownerId

                select inquiry

            ).FirstOrDefaultAsync();
        }

        // 5. Add a new property inquiry
        public async Task AddAsync(
            PropertyInquiry inquiry)
        {
            await _context.PropertyInquiries
                .AddAsync(inquiry);
        }

        // 6. Update an existing inquiry
        public void Update(
            PropertyInquiry inquiry)
        {
            _context.PropertyInquiries
                .Update(inquiry);
        }

        // 7. Save changes to SQL Server
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
