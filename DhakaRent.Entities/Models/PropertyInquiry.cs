using System.ComponentModel.DataAnnotations;
using DhakaRent.Entities.Enums;

namespace DhakaRent.Entities.Models
{
    public class PropertyInquiry
    {
        // Primary Key
        public int Id { get; set; }

        // Tenant Information
        [Required]
        public string TenantId { get; set; } = string.Empty;

        // Related Property
        [Required]
        public int PropertyId { get; set; }

        // Initial Inquiry Message
        [Required]
        [StringLength(2000)]
        public string InitialMessage { get; set; } = string.Empty;

        // Inquiry Status
        public InquiryStatus Status { get; set; }
            = InquiryStatus.Pending;

        // Audit Information
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}