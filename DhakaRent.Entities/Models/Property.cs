using System.ComponentModel.DataAnnotations;
using DhakaRent.Entities.Enums;

namespace DhakaRent.Entities.Models
{
    public class Property
    {
        // Primary Key
        public int Id { get; set; }

        // Property Owner
        [Required]
        public string OwnerId { get; set; } = string.Empty;

        // Basic Property Information
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public PropertyType PropertyType { get; set; } = PropertyType.Apartment;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        // Location Information
        [Required]
        [StringLength(100)]
        public string Area { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        // Property Details
        [Range(1, 100000)]
        public decimal SizeInSqFt { get; set; }

        [Range(0, 50)]
        public int Bedrooms { get; set; }

        [Range(0, 50)]
        public int Bathrooms { get; set; }

        public bool IsFurnished { get; set; }

        // Rental Information
        [Range(1, 100000000)]
        public decimal MonthlyRent { get; set; }

        public DateTime? AvailableFrom { get; set; }

        // Listing Management
        public ListingStatus ListingStatus { get; set; } = ListingStatus.Pending;

        [StringLength(1000)]
        public string? RejectionReason { get; set; }

        // Audit Information
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}