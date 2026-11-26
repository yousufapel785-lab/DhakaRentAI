
using System.Security.Claims;
using DhakaRent.Entities.Enums;
using DhakaRent.Facade.Interfaces;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "Tenant")]
    public class PropertyInquiryController : Controller
    {
        private readonly IPropertyInquiryFacade _inquiryFacade;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMessagingFacade _messagingFacade;

        public PropertyInquiryController(
            IPropertyInquiryFacade inquiryFacade,
            IPropertyRepository propertyRepository,
            IMessagingFacade messagingFacade)
        {
            _inquiryFacade = inquiryFacade;
            _propertyRepository = propertyRepository;
            _messagingFacade = messagingFacade;
        }

        // GET: /PropertyInquiry
        // Show logged-in tenant's inquiries
        // with actual property information.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Step A: Get logged-in Tenant ID.
            string? tenantId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Unauthorized();
            }

            // Step B: Retrieve only this tenant's inquiries.
            var inquiries = await _inquiryFacade
                .GetMineAsync(tenantId);

            // Step C: Retrieve this tenant's conversations.
            var conversations = await _messagingFacade
                .GetMyConversationsAsync(tenantId);

            // Step D: Create Inquiry ID -> Conversation ID mapping.
            var conversationIdsByInquiryId = conversations
                .ToDictionary(
                    c => c.PropertyInquiryId,
                    c => c.Id);

            // Step E: Send conversation mapping to Razor View.
            // Existing Open Conversation functionality
            // will continue working.
            ViewData["ConversationIdsByInquiryId"] =
                conversationIdsByInquiryId;

            // Step F: Collect unique Property IDs
            // from this tenant's authorized inquiries.
            int[] inquiryPropertyIds = inquiries
                .Select(i => i.PropertyId)
                .Distinct()
                .ToArray();

            // Step G: Retrieve property information in bulk.
            // Use the existing _inquiryFacade field.
            var inquiryProperties = await _inquiryFacade
                .GetPropertiesByIdsAsync(inquiryPropertyIds);

            // Step H: Combine each inquiry
            // with its actual property information.
            var inquiryViewModels = inquiries
                .Select(inquiry =>
                {
                    inquiryProperties.TryGetValue(
                        inquiry.PropertyId,
                        out var property);

                    return new PropertyInquiryListItemViewModel
                    {
                        Inquiry = inquiry,

                        PropertyTitle = property?.Title
                            ?? "Property unavailable",

                        PropertyArea = property?.Area
                            ?? string.Empty,

                        PropertyAddress = property?.Address
                            ?? string.Empty,

                        CanViewPropertyDetails =
                            property != null &&
                            property.ListingStatus ==
                                ListingStatus.Approved
                    };
                })
                .ToList();

            // Step I: Send property information to Razor View.
            ViewData["PropertyInquiryItems"] =
                inquiryViewModels;

            // Step J: Keep the original inquiry list model.
            // This avoids breaking the existing Index.cshtml.
            return View(inquiries);
        }

        // GET: /PropertyInquiry/Create?propertyId=1
        // Open Contact Owner form.
        [HttpGet]
        public async Task<IActionResult> Create(int propertyId)
        {
            string? tenantId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Unauthorized();
            }

            if (propertyId <= 0)
            {
                return NotFound();
            }

            // Verify property exists and is approved.
            var property = await _propertyRepository
                .GetByIdAsync(propertyId);

            if (property == null ||
                property.ListingStatus != ListingStatus.Approved)
            {
                return NotFound();
            }

            // Prevent contacting yourself.
            if (property.OwnerId == tenantId)
            {
                return BadRequest(
                    "You cannot inquire about your own property.");
            }

            ViewBag.PropertyId = property.Id;
            ViewBag.PropertyTitle = property.Title;

            return View(new PropertyInquiryFormViewModel());
        }

        // POST: /PropertyInquiry/Create?propertyId=1
        // Send inquiry to property owner.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int propertyId,
            PropertyInquiryFormViewModel model)
        {
            string? tenantId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Unauthorized();
            }

            if (propertyId <= 0)
            {
                return NotFound();
            }

            // Verify the property before processing the form.
            var property = await _propertyRepository
                .GetByIdAsync(propertyId);

            if (property == null ||
                property.ListingStatus != ListingStatus.Approved)
            {
                return NotFound();
            }

            ViewBag.PropertyId = property.Id;
            ViewBag.PropertyTitle = property.Title;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Facade validates the message,
                // approval status and business rules.
                await _inquiryFacade.CreateAsync(
                    propertyId,
                    tenantId,
                    model.InitialMessage);

                TempData["SuccessMessage"] =
                    "Your inquiry has been sent successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
        }
    }
}
