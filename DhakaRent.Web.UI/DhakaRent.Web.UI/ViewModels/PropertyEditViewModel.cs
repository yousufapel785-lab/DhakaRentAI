
using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Web.UI.ViewModels
{
    public class PropertyEditViewModel : PropertyCreateViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Invalid property ID.")]
        public int Id { get; set; }
    }
}
