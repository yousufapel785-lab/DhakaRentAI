
using DhakaRent.Entities.Enums;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;

namespace DhakaRent.Facade.Services
{
    public class MessagingFacade : IMessagingFacade
    {
        private readonly IConversationRepository
            _conversationRepository;

        private readonly IMessageRepository
            _messageRepository;

        private readonly IPropertyInquiryFacade
            _inquiryFacade;

        private readonly IDatabaseTransactionExecutor
            _transactionExecutor;

        public MessagingFacade(
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            IPropertyInquiryFacade inquiryFacade,
            IDatabaseTransactionExecutor transactionExecutor)
        {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _inquiryFacade = inquiryFacade;
            _transactionExecutor = transactionExecutor;
        }

        // 1. Get an authorized conversation
        public async Task<Conversation?> GetConversationAsync(
            int conversationId,
            string userId)
        {
            ValidateUserId(userId);

            if (conversationId <= 0)
            {
                return null;
            }

            return await _conversationRepository
                .GetByIdAndParticipantAsync(
                    conversationId,
                    userId);
        }

        // 2. Get conversations for logged-in user
        public async Task<IReadOnlyList<Conversation>>
            GetMyConversationsAsync(string userId)
        {
            ValidateUserId(userId);

            return await _conversationRepository
                .GetByParticipantAsync(userId);
        }

        // 3. Get message history after authorization
        public async Task<IReadOnlyList<Message>>
            GetMessagesAsync(
                int conversationId,
                string userId)
        {
            await EnsureParticipantAsync(
                conversationId,
                userId);

            return await _messageRepository
                .GetByConversationIdAsync(conversationId);
        }

        // 4. Count unread incoming messages
        public async Task<int> GetUnreadCountAsync(
            string userId)
        {
            ValidateUserId(userId);

            return await _messageRepository
                .GetUnreadCountAsync(userId);
        }

        // Get unread incoming message counts
        // separately for each conversation.
        public async Task<Dictionary<int, int>>
            GetUnreadCountsByConversationAsync(string userId)
        {
            // Step A: Validate logged-in user ID.
            ValidateUserId(userId);

            // Step B: Retrieve unread counts
            // grouped by conversation.
            return await _messageRepository
                .GetUnreadCountsByConversationAsync(userId);
        }

        // Get the latest message preview
        // for each conversation belonging to a user.
        public async Task<Dictionary<int, string>>
            GetLatestMessagePreviewsAsync(string userId)
        {
            // Step A: Validate logged-in user ID.
            ValidateUserId(userId);

            // Step B: Retrieve latest messages
            // from the Message Repository.
            return await _messageRepository
                .GetLatestMessagePreviewsAsync(userId);
        }



        // 5. Mark incoming messages as read
        public async Task MarkAsReadAsync(
            int conversationId,
            string readerId)
        {
            await EnsureParticipantAsync(
                conversationId,
                readerId);

            await _messageRepository.MarkAsReadAsync(
                conversationId,
                readerId);

            await _messageRepository.SaveChangesAsync();
        }

        // 6. PropertyOwner's first reply to an inquiry
        public async Task<int> ReplyToInquiryAsync(
            int inquiryId,
            string ownerId,
            string replyContent)
        {
            ValidateUserId(ownerId);

            if (inquiryId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(inquiryId),
                    "A valid inquiry ID is required.");
            }

            // Validate reply message
            string messageText = ValidateMessage(
                replyContent);

            // Conversation, Message and Inquiry Status
            // are saved inside one database transaction.
            return await _transactionExecutor
                .ExecuteAsync(async () =>
                {
                    // Step A:
                    // Verify the inquiry belongs to
                    // a property owned by this user.
                    var inquiry = await _inquiryFacade
                        .GetReceivedByIdAsync(
                            inquiryId,
                            ownerId);

                    if (inquiry == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Inquiry not found or access denied.");
                    }


                    if (inquiry.Status != InquiryStatus.Pending)
                    {
                        throw new InvalidOperationException(
                            "The first reply is only allowed for a pending inquiry. " +
                            "For an existing conversation, use Send Message.");
                    }


                    // Step B:
                    // Find existing conversation.
                    var conversation =
                        await _conversationRepository
                            .GetByInquiryIdAsync(inquiryId);

                    DateTime now = DateTime.UtcNow;

                    if (conversation == null)
                    {
                        // First reply:
                        // Create a new conversation.
                        conversation = new Conversation
                        {
                            PropertyInquiryId = inquiry.Id,
                            TenantId = inquiry.TenantId,
                            OwnerId = ownerId,
                            CreatedAt = now,
                            IsActive = true
                        };

                        await _conversationRepository
                            .AddAsync(conversation);

                        // Generate Conversation.Id.
                        // This SaveChanges is still inside
                        // the database transaction.
                        await _conversationRepository
                            .SaveChangesAsync();
                    }
                    else
                    {
                        // Verify existing conversation
                        // matches the inquiry participants.
                        if (conversation.OwnerId != ownerId ||
                            conversation.TenantId != inquiry.TenantId)
                        {
                            throw new UnauthorizedAccessException(
                                "Conversation participants do not match.");
                        }

                        if (!conversation.IsActive)
                        {
                            throw new InvalidOperationException(
                                "This conversation is inactive.");
                        }
                    }

                    // Step C:
                    // Create the owner's reply message.
                    var message = new Message
                    {
                        ConversationId = conversation.Id,
                        SenderId = ownerId,
                        Content = messageText,
                        SentAt = now,
                        IsRead = false
                    };

                    await _messageRepository
                        .AddAsync(message);

                    // Step D:
                    // Update conversation timestamp.
                    conversation.LastMessageAt = now;

                    // Step E:
                    // Update inquiry status.
                    inquiry.Status = InquiryStatus.Responded;
                    inquiry.UpdatedAt = now;

                    // Transaction Executor will:
                    // 1. Save pending changes
                    // 2. Commit the transaction
                    // 3. Roll back if an operation fails
                    return conversation.Id;
                });
        }


        // 7. Send message in an existing conversation
        public async Task<int> SendMessageAsync(
            int conversationId,
            string senderId,
            string content)
        {
            // Step A: Validate sender
            ValidateUserId(senderId);

            if (conversationId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(conversationId),
                    "A valid conversation ID is required.");
            }

            // Step B: Validate message content
            // Removes extra spaces and enforces max 3000 chars.
            string messageText = ValidateMessage(content);

            // Step C: Execute database operations
            // inside a single transaction.
            return await _transactionExecutor
                .ExecuteAsync(async () =>
                {
                    // Step D: Verify sender is a participant
                    var conversation =
                        await _conversationRepository
                            .GetByIdAndParticipantAsync(
                                conversationId,
                                senderId);

                    if (conversation == null)
                    {
                        throw new UnauthorizedAccessException(
                            "Conversation not found or access denied.");
                    }

                    // Step E: Verify conversation is active
                    if (!conversation.IsActive)
                    {
                        throw new InvalidOperationException(
                            "Cannot send messages to an inactive conversation.");
                    }

                    DateTime now = DateTime.UtcNow;

                    // Step F: Create new message
                    var message = new Message
                    {
                        ConversationId = conversation.Id,
                        SenderId = senderId,
                        Content = messageText,
                        SentAt = now,
                        IsRead = false
                    };

                    await _messageRepository
                        .AddAsync(message);

                    // Step G: Update last message timestamp
                    conversation.LastMessageAt = now;

                    // Step H: Save inside the transaction
                    // to generate the new Message.Id.
                    await _messageRepository
                        .SaveChangesAsync();

                    // Transaction Executor commits
                    // after all operations succeed.
                    return message.Id;
                });
        }


        // Helper: validate user ID
        private static void ValidateUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "A valid user ID is required.",
                    nameof(userId));
            }
        }

        // Helper: validate message content
        private static string ValidateMessage(
            string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(
                    "Message cannot be empty.",
                    nameof(content));
            }

            string trimmedContent = content.Trim();

            if (trimmedContent.Length > 3000)
            {
                throw new ArgumentException(
                    "Message cannot exceed 3000 characters.",
                    nameof(content));
            }

            return trimmedContent;
        }

        // Helper: verify conversation participant
        private async Task EnsureParticipantAsync(
            int conversationId,
            string userId)
        {
            var conversation = await GetConversationAsync(
                conversationId,
                userId);

            if (conversation == null)
            {
                throw new UnauthorizedAccessException(
                    "Conversation not found or access denied.");
            }
        }
    }
}
