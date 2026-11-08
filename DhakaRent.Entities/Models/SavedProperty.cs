using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Entities.Models
{
    public class SavedProperty
    {
        // Primary Key
        public int Id { get; set; }

        // Tenant Information
        [Required]
        public string TenantId { get; set; } = string.Empty;

        // Property Information
        [Required]
        public int PropertyId { get; set; }

        // Audit Information
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}