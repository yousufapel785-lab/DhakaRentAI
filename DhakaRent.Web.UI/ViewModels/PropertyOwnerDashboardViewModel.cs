namespace DhakaRent.Web.UI.ViewModels
{
    public class PropertyOwnerDashboardViewModel
    {
        public int TotalProperties { get; set; }

        public int PendingProperties { get; set; }

        public int ApprovedProperties { get; set; }

        public int RejectedProperties { get; set; }

        public List<PropertyListItemViewModel> Properties { get; set; }
            = new();
    }

    public class PropertyListItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;

        public decimal MonthlyRent { get; set; }

        public string ListingStatus { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}