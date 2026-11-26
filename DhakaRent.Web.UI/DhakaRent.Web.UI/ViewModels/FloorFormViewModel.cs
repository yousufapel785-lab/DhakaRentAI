
using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Web.UI.ViewModels
{
    public class FloorFormViewModel
    {
        [Display(Name = "Floor Number")]
        public int FloorNumber { get; set; }

        [StringLength(50,
            ErrorMessage = "Floor label cannot exceed 50 characters.")]
        [Display(Name = "Floor Label")]
        public string Label { get; set; } = string.Empty;
    }
}
