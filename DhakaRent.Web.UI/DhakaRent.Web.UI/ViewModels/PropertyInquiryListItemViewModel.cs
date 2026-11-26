
using DhakaRent.Entities.Models;

namespace DhakaRent.Web.UI.ViewModels
{
    public class PropertyInquiryListItemViewModel
    {
        // Original inquiry information
        public PropertyInquiry Inquiry { get; set; } = new();

        // Property details for user-friendly display
        public string PropertyTitle { get; set; }
            = "Property unavailable";

        public string PropertyArea { get; set; }
            = string.Empty;

        public string PropertyAddress { get; set; }
            = string.Empty;

        // Show public property details link
        // only when the listing is publicly available.
        public bool CanViewPropertyDetails { get; set; }
    }
}
