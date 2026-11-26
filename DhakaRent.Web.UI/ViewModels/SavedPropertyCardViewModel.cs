
namespace DhakaRent.Web.UI.ViewModels
{
    public class SavedPropertyCardViewModel
    {
        // Saved property information
        public int PropertyId { get; set; }

        public DateTime SavedAt { get; set; }

        // Property details
        public string Title { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal MonthlyRent { get; set; }

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        // Primary image URL (optional)
        public string? ImageUrl { get; set; }
    }
}
