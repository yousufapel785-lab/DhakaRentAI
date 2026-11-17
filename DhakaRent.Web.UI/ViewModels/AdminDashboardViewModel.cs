
namespace DhakaRent.Web.UI.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProperties { get; set; }

        public int PendingProperties { get; set; }

        public int ApprovedProperties { get; set; }

        public int RejectedProperties { get; set; }

        public List<AdminRecentPendingPropertyViewModel>
            RecentPendingProperties
        { get; set; } = new();
    }

    public class AdminRecentPendingPropertyViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;

        public decimal MonthlyRent { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
