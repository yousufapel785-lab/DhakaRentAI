
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IMessageRepository
    {
        // 1. Get all messages of a conversation
        Task<IReadOnlyList<Message>> GetByConversationIdAsync(
            int conversationId);

        // 2. Count total unread incoming messages
        Task<int> GetUnreadCountAsync(
            string userId);

        // 3. Count unread messages per conversation
        Task<Dictionary<int, int>> GetUnreadCountsByConversationAsync(
            string userId);

        // Get the latest message preview
        // for each conversation belonging to a user.
        // Key = Conversation ID
        // Value = Latest Message Content
        Task<Dictionary<int, string>>
            GetLatestMessagePreviewsAsync(string userId);

        // 4. Mark incoming messages as read
        Task MarkAsReadAsync(
            int conversationId,
            string readerId);

        // 5. Add a new message
        Task AddAsync(
            Message message);

        // 6. Save database changes
        Task SaveChangesAsync();
    }
}
