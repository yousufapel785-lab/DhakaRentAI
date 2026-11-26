
using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Web.UI.ViewModels
{
    public class RentalUnitFormViewModel
    {
        [Required(ErrorMessage = "Unit number is required.")]
        [StringLength(30, ErrorMessage = "Unit number cannot exceed 30 characters.")]
        [Display(Name = "Unit Number")]
        public string UnitNumber { get; set; } = string.Empty;

        [Range(
            typeof(decimal),
            "1",
            "100000",
            ErrorMessage = "Unit size must be between 1 and 100000 square feet.")]
        [Display(Name = "Size (Square Feet)")]
        public decimal SizeInSqFt { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "100000000",
            ErrorMessage = "Monthly rent must be between 0 and 100000000.")]
        [Display(Name = "Standard Monthly Rent (BDT)")]
        public decimal StandardMonthlyRent { get; set; }

        [Range(0, 50, ErrorMessage = "Bedrooms must be between 0 and 50.")]
        [Display(Name = "Bedrooms")]
        public int Bedrooms { get; set; }

        [Range(0, 50, ErrorMessage = "Bathrooms must be between 0 and 50.")]
        [Display(Name = "Bathrooms")]
        public int Bathrooms { get; set; }
    }
}
