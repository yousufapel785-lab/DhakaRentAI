
namespace DhakaRent.Facade.Interfaces
{
    public record SavedPropertyCardData
    {
        // Saved Property Information
        public int PropertyId { get; init; }

        public DateTime SavedAt { get; init; }

        // Property Information
        public string Title { get; init; } = string.Empty;

        public string Area { get; init; } = string.Empty;

        public string Address { get; init; } = string.Empty;

        public decimal MonthlyRent { get; init; }

        public int Bedrooms { get; init; }

        public int Bathrooms { get; init; }

        // Optional Cover Image
        public string? ImageUrl { get; init; }
    }
}
