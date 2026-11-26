
using DhakaRent.DataAccess.Context;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DhakaRent.DataAccess.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly ApplicationDbContext _context;

        public ConversationRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Find conversation by inquiry ID
        public async Task<Conversation?> GetByInquiryIdAsync(
            int inquiryId)
        {
            return await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.PropertyInquiryId == inquiryId);
        }

        // 2. Get conversation only if the user is a participant
        public async Task<Conversation?> GetByIdAndParticipantAsync(
            int conversationId,
            string userId)
        {
            return await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    (c.TenantId == userId ||
                     c.OwnerId == userId));
        }

        // 3. Get conversations belonging to a participant
        public async Task<IReadOnlyList<Conversation>>
            GetByParticipantAsync(string userId)
        {
            return await _context.Conversations
                .AsNoTracking()
                .Where(c =>
                    c.TenantId == userId ||
                    c.OwnerId == userId)
                .OrderByDescending(c =>
                    c.LastMessageAt ?? c.CreatedAt)
                .ThenByDescending(c => c.Id)
                .ToListAsync();
        }

        // 4. Add new conversation
        public async Task AddAsync(
            Conversation conversation)
        {
            await _context.Conversations
                .AddAsync(conversation);
        }

        // 5. Save database changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
