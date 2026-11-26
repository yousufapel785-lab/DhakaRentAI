
using DhakaRent.Entities.Models;

namespace DhakaRent.Facade.Interfaces
{
    public interface IConversationRepository
    {
        // Get conversation linked to an inquiry
        Task<Conversation?> GetByInquiryIdAsync(
            int inquiryId);

        // Get conversation only if user is a participant
        Task<Conversation?> GetByIdAndParticipantAsync(
            int conversationId,
            string userId);

        // Get conversations for tenant or property owner
        Task<IReadOnlyList<Conversation>> GetByParticipantAsync(
            string userId);

        // Add a new conversation
        Task AddAsync(Conversation conversation);

        // Save changes
        Task SaveChangesAsync();
    }
}
