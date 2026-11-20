
using DhakaRent.DataAccess.Identity;
using DhakaRent.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DhakaRent.DataAccess.Context
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Business Entity DbSets

        public DbSet<Property> Properties { get; set; }

        public DbSet<PropertyImage> PropertyImages { get; set; }

        public DbSet<SavedProperty> SavedProperties { get; set; }

        public DbSet<PropertyInquiry> PropertyInquiries { get; set; }

        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<ViewingRequest> ViewingRequests { get; set; }

        public DbSet<PropertyReport> PropertyReports { get; set; }

        public DbSet<RentPredictionHistory>
            RentPredictionHistories
        { get; set; }

        // Property Management SaaS Entities

        public DbSet<Building> Buildings { get; set; }

        public DbSet<Floor> Floors { get; set; }

        public DbSet<RentalUnit> RentalUnits { get; set; }


        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // 1. Property Configuration

            builder.Entity<Property>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Property>()
                .Property(p => p.MonthlyRent)
                .HasPrecision(18, 2);

            builder.Entity<Property>()
                .Property(p => p.SizeInSqFt)
                .HasPrecision(10, 2);


            // 2. PropertyImage Configuration

            builder.Entity<PropertyImage>()
                .HasOne<Property>()
                .WithMany()
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);


            // 3. SavedProperty Configuration

            builder.Entity<SavedProperty>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(sp => sp.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SavedProperty>()
                .HasOne<Property>()
                .WithMany()
                .HasForeignKey(sp => sp.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SavedProperty>()
                .HasIndex(sp => new
                {
                    sp.TenantId,
                    sp.PropertyId
                })
                .IsUnique();


            // 4. PropertyInquiry Configuration

            builder.Entity<PropertyInquiry>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(pi => pi.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PropertyInquiry>()
                .HasOne<Property>()
                .WithMany()
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);


            // 5. Conversation Configuration

            builder.Entity<Conversation>()
                .HasOne<PropertyInquiry>()
                .WithOne()
                .HasForeignKey<Conversation>(
                    c => c.PropertyInquiryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Conversation>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Conversation>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);


            // 6. Message Configuration

            builder.Entity<Message>()
                .HasOne<Conversation>()
                .WithMany()
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);


            // 7. ViewingRequest Configuration

            builder.Entity<ViewingRequest>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(vr => vr.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ViewingRequest>()
                .HasOne<Property>()
                .WithMany()
                .HasForeignKey(vr => vr.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);


            // 8. PropertyReport Configuration

            builder.Entity<PropertyReport>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(pr => pr.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PropertyReport>()
                .HasOne<Property>()
                .WithMany()
                .HasForeignKey(pr => pr.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PropertyReport>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(pr => pr.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);


            // 9. RentPredictionHistory Configuration

            builder.Entity<RentPredictionHistory>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(rp => rp.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RentPredictionHistory>()
                .Property(rp => rp.PredictedRent)
                .HasPrecision(18, 2);


            // 10. Building Configuration

            builder.Entity<Building>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Building>()
                .HasIndex(b => b.OwnerId);

            builder.Entity<Building>()
                .Property(b => b.Latitude)
                .HasPrecision(9, 6);

            builder.Entity<Building>()
                .Property(b => b.Longitude)
                .HasPrecision(9, 6);


            // 11. Floor Configuration

            builder.Entity<Floor>()
                .HasOne(f => f.Building)
                .WithMany(b => b.Floors)
                .HasForeignKey(f => f.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Floor>()
                .HasIndex(f => new
                {
                    f.BuildingId,
                    f.FloorNumber
                })
                .IsUnique();


            // 12. RentalUnit Configuration

            builder.Entity<RentalUnit>()
                .HasOne(u => u.Floor)
                .WithMany(f => f.Units)
                .HasForeignKey(u => u.FloorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RentalUnit>()
                .HasIndex(u => new
                {
                    u.FloorId,
                    u.UnitNumber
                })
                .IsUnique();

            builder.Entity<RentalUnit>()
                .Property(u => u.SizeInSqFt)
                .HasPrecision(10, 2);

            builder.Entity<RentalUnit>()
                .Property(u => u.StandardMonthlyRent)
                .HasPrecision(18, 2);


            // Useful Indexes

            builder.Entity<Property>()
                .HasIndex(p => p.OwnerId);

            builder.Entity<Property>()
                .HasIndex(p => new
                {
                    p.ListingStatus,
                    p.Area
                });

            builder.Entity<PropertyInquiry>()
                .HasIndex(pi => pi.TenantId);

            builder.Entity<Message>()
                .HasIndex(m => new
                {
                    m.ConversationId,
                    m.SentAt
                });

            builder.Entity<ViewingRequest>()
                .HasIndex(vr => new
                {
                    vr.PropertyId,
                    vr.RequestedDateTime
                });

            builder.Entity<PropertyReport>()
                .HasIndex(pr => pr.Status);

            builder.Entity<RentPredictionHistory>()
                .HasIndex(rp => new
                {
                    rp.TenantId,
                    rp.PredictedAt
                });
        }
    }
}
