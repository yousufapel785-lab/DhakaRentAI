
namespace DhakaRent.Web.UI.ViewModels
{
    public class PublicPropertyDetailsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string PropertyType { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal SizeInSqFt { get; set; }

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        public bool IsFurnished { get; set; }

        public decimal MonthlyRent { get; set; }

        public DateTime? AvailableFrom { get; set; }
    }
}
