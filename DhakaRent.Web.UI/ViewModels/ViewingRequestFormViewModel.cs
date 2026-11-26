
using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Web.UI.ViewModels
{
    public class ViewingRequestFormViewModel
    {
        // Property being requested for viewing
        [Range(1, int.MaxValue,
            ErrorMessage = "A valid property is required.")]
        public int PropertyId { get; set; }

        // Preferred date and time in Bangladesh local time
        [Required(
            ErrorMessage = "Please select a viewing date and time.")]
        [DataType(DataType.DateTime)]
        [Display(
            Name = "Preferred Viewing Date & Time (Bangladesh Time)")]
        public DateTime? RequestedDateTimeLocal { get; set; }

        // Optional message for the property owner
        [StringLength(1000,
            ErrorMessage = "Message cannot exceed 1000 characters.")]
        [Display(Name = "Message to Property Owner (Optional)")]
        public string? TenantMessage { get; set; }
    }
}
