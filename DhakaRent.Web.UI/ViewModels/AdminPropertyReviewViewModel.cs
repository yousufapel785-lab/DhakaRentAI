
namespace DhakaRent.Web.UI.ViewModels
{
    public class AdminPropertyReviewViewModel
    {
        // Property Information
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string PropertyType { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Location Information
        public string Area { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // Property Specifications
        public decimal SizeInSqFt { get; set; }

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        public bool IsFurnished { get; set; }

        // Rental Information
        public decimal MonthlyRent { get; set; }

        public DateTime? AvailableFrom { get; set; }

        // Owner Information
        public string OwnerName { get; set; } = string.Empty;

        public string OwnerEmail { get; set; } = string.Empty;

        // Listing Information
        public string ListingStatus { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string? RejectionReason { get; set; }
    }
}
