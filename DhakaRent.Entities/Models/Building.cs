
using System;
using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Entities.Models
{
    public class Building
    {
        // Primary Key
        public int Id { get; set; }

        // Building Owner
        [Required]
        public string OwnerId { get; set; } = string.Empty;

        // Building Information
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Area { get; set; } = string.Empty;

        // Optional Location Coordinates
        [Range(typeof(decimal), "-90", "90")]
        public decimal? Latitude { get; set; }

        [Range(typeof(decimal), "-180", "180")]
        public decimal? Longitude { get; set; }

        // Soft Delete / Archive
        public bool IsArchived { get; set; } = false;

        // Audit Information
        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        // One Building can have multiple Floors
        public ICollection<Floor> Floors { get; set; }
            = new List<Floor>();

    }
}
