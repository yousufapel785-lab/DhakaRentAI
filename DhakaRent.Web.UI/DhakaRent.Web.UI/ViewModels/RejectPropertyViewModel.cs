using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Web.UI.ViewModels
{
    public class RejectPropertyViewModel
    {
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "Rejection reason is required.")]
        [StringLength(500,
            ErrorMessage = "Rejection reason cannot exceed 500 characters.")]
        [Display(Name = "Rejection Reason")]
        public string Reason { get; set; } = string.Empty;
    }
}