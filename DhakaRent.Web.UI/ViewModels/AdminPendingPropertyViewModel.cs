namespace DhakaRent.Web.UI.ViewModels
{
    public class AdminPendingPropertyViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string PropertyType { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal MonthlyRent { get; set; }

        public decimal SizeInSqFt { get; set; }

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}