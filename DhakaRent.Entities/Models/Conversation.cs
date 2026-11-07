using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Entities.Models
{
    public class Conversation
    {
        // Primary Key
        public int Id { get; set; }

        // Related Inquiry
        [Required]
        public int PropertyInquiryId { get; set; }

        // Conversation Participants
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        // Conversation Information
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastMessageAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}