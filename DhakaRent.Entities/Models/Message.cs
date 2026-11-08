using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Entities.Models
{
    public class Message
    {
        // Primary Key
        public int Id { get; set; }

        // Related Conversation
        [Required]
        public int ConversationId { get; set; }

        // Message Sender
        [Required]
        public string SenderId { get; set; } = string.Empty;

        // Message Content
        [Required]
        [StringLength(3000)]
        public string Content { get; set; } = string.Empty;

        // Message Information
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}