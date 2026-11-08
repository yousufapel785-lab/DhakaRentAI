
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DhakaRent.DataAccess.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}
