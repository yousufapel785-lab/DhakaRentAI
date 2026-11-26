
using DhakaRent.Entities.Models;

namespace DhakaRent.Web.UI.ViewModels
{
    public class ViewingRequestListItemViewModel
    {
        // Original viewing request information
        public ViewingRequest Request { get; set; } = new();

        // Property information for user-friendly display
        public string PropertyTitle { get; set; }
            = "Property unavailable";

        public string PropertyArea { get; set; }
            = string.Empty;

        public string PropertyAddress { get; set; }
            = string.Empty;

        // Whether a public property details link
        // should be displayed
        public bool CanViewPropertyDetails { get; set; }
    }
}
