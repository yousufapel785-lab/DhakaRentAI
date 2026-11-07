using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Entities.Models
{
    public class PropertyImage
    {
        // Primary Key
        public int Id { get; set; }

        // Related Property
        [Required]
        public int PropertyId { get; set; }

        // Image Information
        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        // Audit Information
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}