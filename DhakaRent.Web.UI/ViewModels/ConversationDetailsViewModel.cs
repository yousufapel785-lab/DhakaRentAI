
using DhakaRent.Entities.Models;

namespace DhakaRent.Web.UI.ViewModels
{
    public class ConversationDetailsViewModel
    {
        // The conversation between Tenant and Owner
        public required Conversation Conversation
        {
            get;
            init;
        }

        // Original property inquiry
        public required PropertyInquiry Inquiry
        {
            get;
            init;
        }

        // Messages sent after the initial inquiry
        public IReadOnlyList<Message> Messages
        {
            get;
            init;
        } = new List<Message>();

        // Logged-in user's ID
        // Used to identify sent and received messages
        public required string CurrentUserId
        {
            get;
            init;
        }
    }
}
