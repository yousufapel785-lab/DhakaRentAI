
using DhakaRent.DataAccess.Context;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DhakaRent.DataAccess.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public MessageRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Get message history of a conversation
        public async Task<IReadOnlyList<Message>>
            GetByConversationIdAsync(int conversationId)
        {
            return await _context.Messages
                .AsNoTracking()
                .Where(m =>
                    m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ThenBy(m => m.Id)
                .ToListAsync();
        }


        // Get unread incoming message counts
        // grouped by conversation.
        public async Task<Dictionary<int, int>>
            GetUnreadCountsByConversationAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "A valid user ID is required.",
                    nameof(userId));
            }

            var unreadCounts = await (
                from message in _context.Messages.AsNoTracking()

                join conversation in
                    _context.Conversations.AsNoTracking()
                    on message.ConversationId equals conversation.Id

                where
                    // Only conversations belonging to this user
                    (conversation.TenantId == userId ||
                     conversation.OwnerId == userId)

                    // Only messages from the other participant
                    && message.SenderId != userId

                    // Only unread messages
                    && !message.IsRead

                // Group unread messages by conversation
                group message by message.ConversationId into messageGroup

                select new
                {
                    ConversationId = messageGroup.Key,
                    UnreadCount = messageGroup.Count()
                }

            ).ToDictionaryAsync(
                x => x.ConversationId,
                x => x.UnreadCount);

            return unreadCounts;
        }

        // Get the latest message content
        // for each conversation belonging to a user.
        public async Task<Dictionary<int, string>>
            GetLatestMessagePreviewsAsync(string userId)
        {
            // Step A: Validate user ID.
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "A valid user ID is required.",
                    nameof(userId));
            }

            // Step B: Retrieve the latest message
            // for each conversation belonging to this user.
            var latestMessages = await _context.Conversations
                .AsNoTracking()
                .Where(c =>
                    c.TenantId == userId ||
                    c.OwnerId == userId)
                .Select(c => new
                {
                    ConversationId = c.Id,

                    LatestContent = _context.Messages
                        .AsNoTracking()
                        .Where(m =>
                            m.ConversationId == c.Id)
                        .OrderByDescending(m => m.SentAt)
                        .ThenByDescending(m => m.Id)
                        .Select(m => m.Content)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Step C: Build a dictionary.
            // Key = Conversation ID
            // Value = Latest Message Content
            return latestMessages
                .Where(x => x.LatestContent != null)
                .ToDictionary(
                    x => x.ConversationId,
                    x => x.LatestContent!);
        }



        // 2. Count unread incoming messages
        public async Task<int> GetUnreadCountAsync(
            string userId)
        {
            return await (
                from message in _context.Messages
                join conversation in _context.Conversations
                    on message.ConversationId equals conversation.Id

                where
                    (conversation.TenantId == userId ||
                     conversation.OwnerId == userId)

                    && message.SenderId != userId
                    && !message.IsRead

                select message.Id
            ).CountAsync();
        }

        // 3. Mark incoming messages as read
        public async Task MarkAsReadAsync(
            int conversationId,
            string readerId)
        {
            var unreadMessages = await (
                from message in _context.Messages
                join conversation in _context.Conversations
                    on message.ConversationId equals conversation.Id

                where
                    conversation.Id == conversationId

                    && (conversation.TenantId == readerId ||
                        conversation.OwnerId == readerId)

                    && message.SenderId != readerId
                    && !message.IsRead

                select message
            ).ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            // SaveChangesAsync() will be called
            // by the service/facade layer.
        }

        // 4. Add a new message
        public async Task AddAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
        }

        // 5. Save database changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
