
using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Web.UI.ViewModels
{
    public class PropertyInquiryFormViewModel
    {
        [Required(
            ErrorMessage = "Please write a message to the property owner.")]
        [StringLength(
            2000,
            ErrorMessage = "Message cannot exceed 2000 characters.")]
        [Display(Name = "Your Message")]
        public string InitialMessage { get; set; } = string.Empty;
    }
}
