
using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Web.UI.ViewModels
{
    public class BuildingFormViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Building name is required.")]
        [StringLength(150)]
        [Display(Name = "Building Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area is required.")]
        [StringLength(100)]
        public string Area { get; set; } = string.Empty;

        [Range(typeof(decimal), "-90", "90")]
        public decimal? Latitude { get; set; }

        [Range(typeof(decimal), "-180", "180")]
        public decimal? Longitude { get; set; }

        // Both coordinates must be provided together
        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (Latitude.HasValue != Longitude.HasValue)
            {
                yield return new ValidationResult(
                    "Please provide both latitude and longitude, or leave both empty.",
                    new[] { nameof(Latitude), nameof(Longitude) });
            }
        }
    }
}
