
using System.Security.Claims;
using DhakaRent.Facade.Interfaces;
using DhakaRent.Entities.Models;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "Tenant,PropertyOwner")]
    public class MessagingController : Controller
    {
        private readonly IMessagingFacade _messagingFacade;
        private readonly IPropertyInquiryFacade _inquiryFacade;

        public MessagingController(
            IMessagingFacade messagingFacade,
            IPropertyInquiryFacade inquiryFacade)
        {
            _messagingFacade = messagingFacade;
            _inquiryFacade = inquiryFacade;
        }




        // GET: /Messaging
        // Show conversations with unread message counts.
        [HttpGet]

        // GET: /Messaging
        // Show conversations, unread counts
        // and latest message previews.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Step A: Get logged-in user's ID.
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            // Step B: Get logged-in user's conversations.
            var conversations = await _messagingFacade
                .GetMyConversationsAsync(userId);

            // Step C: Get total unread incoming messages.
            int unreadCount = await _messagingFacade
                .GetUnreadCountAsync(userId);

            // Step D: Get unread counts grouped by conversation.
            Dictionary<int, int> unreadCountsByConversation =
                await _messagingFacade
                    .GetUnreadCountsByConversationAsync(userId);

            // Step E: Get latest message previews.
            Dictionary<int, string> latestMessagePreviews =
                await _messagingFacade
                    .GetLatestMessagePreviewsAsync(userId);

            // Step F: Pass total unread count to the View.
            ViewData["UnreadMessageCount"] = unreadCount;

            // Step G: Pass per-conversation unread counts.
            ViewData["UnreadCountsByConversation"] =
                unreadCountsByConversation;

            // Step H: Pass latest message previews.
            ViewData["LatestMessagePreviews"] =
                latestMessagePreviews;

            // Step I: Return existing conversation list.
            return View(conversations);
        }



        // GET: /Messaging/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Step A: Get logged-in user ID.
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            if (id <= 0)
            {
                return NotFound();
            }

            // Step B: Verify conversation participation.
            var conversation = await _messagingFacade
                .GetConversationAsync(id, userId);

            if (conversation == null)
            {
                return NotFound();
            }

            // Step C: Retrieve the original inquiry
            // using role-specific ownership checks.
            PropertyInquiry? inquiry;

            if (User.IsInRole("Tenant") &&
                conversation.TenantId == userId)
            {
                inquiry = await _inquiryFacade
                    .GetMineByIdAsync(
                        conversation.PropertyInquiryId,
                        userId);
            }
            else if (User.IsInRole("PropertyOwner") &&
                     conversation.OwnerId == userId)
            {
                inquiry = await _inquiryFacade
                    .GetReceivedByIdAsync(
                        conversation.PropertyInquiryId,
                        userId);
            }
            else
            {
                return NotFound();
            }

            if (inquiry == null)
            {
                return NotFound();
            }

            // Step D: Verify inquiry and conversation match.
            if (inquiry.Id != conversation.PropertyInquiryId ||
                inquiry.TenantId != conversation.TenantId)
            {
                return NotFound();
            }


            // Step E: Get authorized message history.
            var messages = await _messagingFacade
                .GetMessagesAsync(id, userId);

            // Step F: Mark incoming unread messages as read.
            // Only messages sent by the OTHER participant
            // will be marked as read.
            await _messagingFacade
                .MarkAsReadAsync(id, userId);

            // Step G: Prepare the view model.
            var model = new ConversationDetailsViewModel
            {
                Conversation = conversation,
                Inquiry = inquiry,
                Messages = messages,
                CurrentUserId = userId
            };


            return View(model);
        }

        // POST: /Messaging/SendMessage
        // Tenant or PropertyOwner sends a new message.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(
            int conversationId,
            string? content)
        {
            // Step A: Get logged-in user's ID.
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            // Step B: Validate conversation ID.
            if (conversationId <= 0)
            {
                return NotFound();
            }

            // Step C: Validate message content.
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] =
                    "Please write a message.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = conversationId });
            }

            string messageText = content.Trim();

            if (messageText.Length > 3000)
            {
                TempData["ErrorMessage"] =
                    "Message cannot exceed 3000 characters.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = conversationId });
            }

            try
            {
                // Step D: Send message using Facade.
                // Facade verifies participant authorization,
                // checks conversation status
                // and saves the message.
                await _messagingFacade.SendMessageAsync(
                    conversationId,
                    userId,
                    messageText);

                // Step E: Success feedback.
                TempData["SuccessMessage"] =
                    "Message sent successfully.";

                // Step F: Return to conversation.
                return RedirectToAction(
                    nameof(Details),
                    new { id = conversationId });
            }
            catch (UnauthorizedAccessException)
            {
                // Do not expose private conversations.
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction(
                    nameof(Details),
                    new { id = conversationId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction(
                    nameof(Details),
                    new { id = conversationId });
            }
        }

    }
}
