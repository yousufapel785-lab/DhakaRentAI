
using System.Security.Claims;
using DhakaRent.Facade.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "PropertyOwner")]
    public class OwnerInquiryController : Controller
    {
        private readonly IPropertyInquiryFacade _inquiryFacade;
        private readonly IMessagingFacade _messagingFacade;

        public OwnerInquiryController(
            IPropertyInquiryFacade inquiryFacade,
            IMessagingFacade messagingFacade)
        {
            _inquiryFacade = inquiryFacade;
            _messagingFacade = messagingFacade;
        }


        // GET: /OwnerInquiry
        // Show received inquiries with conversation mapping.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Step A: Get logged-in PropertyOwner ID.
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            // Step B: Get inquiries for this owner's properties.
            var inquiries = await _inquiryFacade
                .GetReceivedAsync(ownerId);

            // Step C: Get this owner's conversations.
            var conversations = await _messagingFacade
                .GetMyConversationsAsync(ownerId);

            // Step D: Map Inquiry ID to Conversation ID.
            var conversationIdsByInquiryId = conversations
                .ToDictionary(
                    c => c.PropertyInquiryId,
                    c => c.Id);

            // Step E: Pass mapping to Razor View.
            ViewData["ConversationIdsByInquiryId"] =
                conversationIdsByInquiryId;

            // Step F: Keep existing inquiries model.
            return View(inquiries);
        }


        // POST: /OwnerInquiry/Reply
        // PropertyOwner sends a reply to a tenant inquiry.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(
            int inquiryId,
            string? replyContent)
        {
            // Step A: Get logged-in PropertyOwner ID.
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            // Step B: Validate inquiry ID.
            if (inquiryId <= 0)
            {
                return NotFound();
            }

            // Step C: Validate reply content.
            if (string.IsNullOrWhiteSpace(replyContent))
            {
                TempData["ErrorMessage"] =
                    "Please write a reply message.";

                return RedirectToAction(nameof(Index));
            }

            if (replyContent.Trim().Length > 3000)
            {
                TempData["ErrorMessage"] =
                    "Reply cannot exceed 3000 characters.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Step D: Send reply using MessagingFacade.
                // Facade verifies actual property ownership,
                // creates/reuses conversation,
                // saves message and updates inquiry status.
                await _messagingFacade.ReplyToInquiryAsync(
                    inquiryId,
                    ownerId,
                    replyContent);

                // Step E: Show success feedback.
                TempData["SuccessMessage"] =
                    "Your reply has been sent successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException)
            {
                // Do not expose another owner's inquiry.
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }
    }
}
