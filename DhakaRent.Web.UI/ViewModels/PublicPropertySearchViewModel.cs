
using DhakaRent.Entities.Enums;

namespace DhakaRent.Web.UI.ViewModels
{
    public class PublicPropertySearchViewModel
    {
        public string? Area { get; set; }

        public PropertyType? PropertyType { get; set; }

        public decimal? MinRent { get; set; }

        public decimal? MaxRent { get; set; }

        public int? Bedrooms { get; set; }

        public List<PublicPropertyListItemViewModel> Properties
        { get; set; } = new();
    }
}
