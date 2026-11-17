
using System.ComponentModel.DataAnnotations;
using DhakaRent.Entities.Enums;

namespace DhakaRent.Web.UI.ViewModels
{
    public class PropertyCreateViewModel : IValidatableObject
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Property Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Property Type")]
        public PropertyType PropertyType { get; set; }
            = PropertyType.Apartment;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Area { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Range(1, 100000)]
        [Display(Name = "Size (Square Feet)")]
        public decimal SizeInSqFt { get; set; }

        [Range(0, 50)]
        public int Bedrooms { get; set; }

        [Range(0, 50)]
        public int Bathrooms { get; set; }

        [Display(Name = "Furnished")]
        public bool IsFurnished { get; set; }

        [Range(1, 100000000)]
        [Display(Name = "Monthly Rent (BDT)")]
        public decimal MonthlyRent { get; set; }

        [Display(Name = "Available From")]
        public DateTime? AvailableFrom { get; set; }


        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            // Current date in Bangladesh (UTC+6)
            DateTime todayInBangladesh =
                DateTime.UtcNow.AddHours(6).Date;

            // Check if selected date is in the past
            if (AvailableFrom.HasValue &&
                AvailableFrom.Value.Date < todayInBangladesh)
            {
                yield return new ValidationResult(
                    "Available From cannot be a past date.",
                    new[] { nameof(AvailableFrom) }
                );
            }
        }

    }
}
