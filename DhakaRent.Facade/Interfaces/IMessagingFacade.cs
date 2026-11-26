
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IMessagingFacade
    {
        // 1. Owner replies to a property inquiry.
        // Creates a conversation if one does not exist.
        // Returns the conversation ID.
        Task<int> ReplyToInquiryAsync(
            int inquiryId,
            string ownerId,
            string replyContent);

        // 2. Tenant or Owner sends a message
        // in an existing conversation.
        // Returns the new message ID.
        Task<int> SendMessageAsync(
            int conversationId,
            string senderId,
            string content);

        // 3. Get a conversation only when
        // the user is a participant.
        Task<Conversation?> GetConversationAsync(
            int conversationId,
            string userId);

        // 4. Get the logged-in user's conversations.
        Task<IReadOnlyList<Conversation>>
            GetMyConversationsAsync(
                string userId);

        // 5. Get message history after
        // verifying participant access.
        Task<IReadOnlyList<Message>> GetMessagesAsync(
            int conversationId,
            string userId);

        // 6. Count unread incoming messages.
        Task<int> GetUnreadCountAsync(
            string userId);

        // Get unread incoming message counts
        // separately for each conversation.
        // Key = Conversation ID
        // Value = Unread Message Count
        Task<Dictionary<int, int>>
            GetUnreadCountsByConversationAsync(
                string userId);

        // Get the latest message preview
        // for each conversation belonging to the user.
        //
        // Key   = Conversation ID
        // Value = Latest Message Content
        Task<Dictionary<int, string>>
            GetLatestMessagePreviewsAsync(string userId);


        // 7. Mark incoming messages as read.
        Task MarkAsReadAsync(
            int conversationId,
            string readerId);
    }
}
