using System.ComponentModel.DataAnnotations;
using DhakaRent.Entities.Enums;

namespace DhakaRent.Entities.Models
{
    public class PropertyReport
    {
        // Primary Key
        public int Id { get; set; }

        // Reporting User
        [Required]
        public string ReporterId { get; set; } = string.Empty;

        // Reported Property
        [Required]
        public int PropertyId { get; set; }

        // Report Information
        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        // Admin Review
        public ReportStatus Status { get; set; }
            = ReportStatus.Pending;

        public string? ReviewedByAdminId { get; set; }

        [StringLength(1000)]
        public string? AdminRemarks { get; set; }

        public DateTime? ReviewedAt { get; set; }

        // Audit Information
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}