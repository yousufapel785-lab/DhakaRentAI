using System.ComponentModel.DataAnnotations;
using DhakaRent.Entities.Enums;

namespace DhakaRent.Entities.Models
{
    public class ViewingRequest
    {
        // Primary Key
        public int Id { get; set; }

        // Tenant and Property Information
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [Required]
        public int PropertyId { get; set; }

        // Requested Viewing Date and Time
        [Required]
        public DateTime RequestedDateTime { get; set; }

        // Owner's Suggested Alternative Time
        public DateTime? ProposedDateTime { get; set; }

        // Additional Information
        [StringLength(1000)]
        public string? TenantMessage { get; set; }

        [StringLength(1000)]
        public string? OwnerResponse { get; set; }

        // Request Status
        public ViewingRequestStatus Status { get; set; }
            = ViewingRequestStatus.Pending;

        // Audit Information
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}