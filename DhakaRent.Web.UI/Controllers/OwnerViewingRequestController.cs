
using System.Security.Claims;
using DhakaRent.Facade.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DhakaRent.Web.UI.ViewModels;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "PropertyOwner")]
    public class OwnerViewingRequestController : Controller
    {
        private readonly IViewingRequestFacade
            _viewingRequestFacade;

        public OwnerViewingRequestController(
            IViewingRequestFacade viewingRequestFacade)
        {
            _viewingRequestFacade = viewingRequestFacade;
        }


        // GET: /OwnerViewingRequest/Index
        // PropertyOwner sees viewing requests
        // with actual property information.
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

            // Step B: Retrieve only this Owner's
            // received viewing requests.
            var requests = await _viewingRequestFacade
                .GetReceivedAsync(ownerId);

            // Step C: Collect unique Property IDs
            // from the authorized viewing requests.
            int[] propertyIds = requests
                .Select(r => r.PropertyId)
                .Distinct()
                .ToArray();

            // Step D: Retrieve property details in bulk.
            var properties = await _viewingRequestFacade
                .GetPropertiesByIdsAsync(propertyIds);

            // Step E: Combine viewing requests
            // with actual property information.
            var viewModels = requests.Select(request =>
            {
                properties.TryGetValue(
                    request.PropertyId,
                    out var property);

                return new ViewingRequestListItemViewModel
                {
                    Request = request,

                    PropertyTitle = property?.Title
                        ?? "Property unavailable",

                    PropertyArea = property?.Area
                        ?? string.Empty,

                    PropertyAddress = property?.Address
                        ?? string.Empty,

                    CanViewPropertyDetails =
                        property != null &&
                        property.ListingStatus ==
                            DhakaRent.Entities.Enums.ListingStatus.Approved
                };

            }).ToList();

            // Step F: Send enriched property information
            // separately to the existing Razor View.
            ViewData["ViewingRequestItems"] = viewModels;

            // Step G: Keep the existing View Model
            // unchanged to prevent model mismatch.
            return View(requests);
        }


        // POST: /OwnerViewingRequest/Accept
        // PropertyOwner accepts a pending viewing request.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(
            int requestId,
            string? ownerResponse)
        {
            // Step A: Get logged-in PropertyOwner ID.
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            // Step B: Validate request ID.
            if (requestId <= 0)
            {
                return BadRequest(
                    "A valid viewing request ID is required.");
            }

            try
            {
                // Step C: Accept using the Facade.
                // Facade checks property ownership,
                // Pending status and future viewing time.
                await _viewingRequestFacade.AcceptAsync(
                    requestId,
                    ownerId,
                    ownerResponse);

                // Step D: Store success notification.
                TempData["SuccessMessage"] =
                    "Viewing request accepted successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                // Do not expose requests belonging
                // to other PropertyOwners.
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                // Request may have already been handled,
                // or the requested viewing time has passed.
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (ArgumentException ex)
            {
                // Invalid owner response, such as
                // exceeding the 1000-character limit.
                TempData["ErrorMessage"] = ex.Message;
            }

            // Step E: Return to received viewing requests.
            return RedirectToAction(nameof(Index));
        }

        // POST: /OwnerViewingRequest/Reject
        // PropertyOwner rejects a pending viewing request.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            int requestId,
            string? ownerResponse)
        {
            // Step A: Get logged-in PropertyOwner ID.
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            // Step B: Validate viewing request ID.
            if (requestId <= 0)
            {
                return BadRequest(
                    "A valid viewing request ID is required.");
            }

            try
            {
                // Step C: Reject using the Facade.
                // Facade validates:
                // - Property ownership
                // - Pending request status
                // - Optional owner response (max 1000 chars)
                await _viewingRequestFacade.RejectAsync(
                    requestId,
                    ownerId,
                    ownerResponse);

                // Step D: Store success notification.
                TempData["SuccessMessage"] =
                    "Viewing request rejected successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                // Do not expose requests belonging
                // to another PropertyOwner.
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                // Request has already been handled
                // or is not in Pending status.
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (ArgumentException ex)
            {
                // Invalid owner response or arguments.
                TempData["ErrorMessage"] = ex.Message;
            }

            // Step E: Return to received viewing requests.
            return RedirectToAction(nameof(Index));
        }

        // POST: /OwnerViewingRequest/ProposeReschedule
        // PropertyOwner proposes an alternative viewing time.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProposeReschedule(
            int requestId,
            DateTime? proposedDateTimeLocal,
            string? ownerResponse)
        {
            // Step A: Get logged-in PropertyOwner ID.
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            // Step B: Validate viewing request ID.
            if (requestId <= 0)
            {
                return BadRequest(
                    "A valid viewing request ID is required.");
            }

            // Step C: Validate proposed date and time.
            if (!ModelState.IsValid ||
                !proposedDateTimeLocal.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Please select a valid proposed viewing date and time.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Step D: Get Bangladesh time zone.
                string timeZoneId = OperatingSystem.IsWindows()
                    ? "Bangladesh Standard Time"
                    : "Asia/Dhaka";

                TimeZoneInfo bangladeshTimeZone =
                    TimeZoneInfo.FindSystemTimeZoneById(
                        timeZoneId);

                // Step E: Treat form input as Bangladesh local time.
                DateTime localDateTime =
                    DateTime.SpecifyKind(
                        proposedDateTimeLocal.Value,
                        DateTimeKind.Unspecified);

                // Step F: Convert Bangladesh time to UTC.
                DateTime proposedDateTimeUtc =
                    TimeZoneInfo.ConvertTimeToUtc(
                        localDateTime,
                        bangladeshTimeZone);

                // Step G: Validate future proposed time.
                if (proposedDateTimeUtc <= DateTime.UtcNow)
                {
                    TempData["ErrorMessage"] =
                        "Please select a future viewing date and time.";

                    return RedirectToAction(nameof(Index));
                }

                // Step H: Propose reschedule using Facade.
                // Facade validates:
                // - Property ownership
                // - Pending request status
                // - Future proposed time
                // - Proposed time differs from original time
                // - Owner response length
                await _viewingRequestFacade
                    .ProposeRescheduleAsync(
                        requestId,
                        ownerId,
                        proposedDateTimeUtc,
                        ownerResponse);

                // Step I: Store success notification.
                TempData["SuccessMessage"] =
                    "Alternative viewing time proposed successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                // Do not expose another owner's requests.
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            // Step J: Return to received viewing requests.
            return RedirectToAction(nameof(Index));
        }

        // POST: /OwnerViewingRequest/Complete
        // PropertyOwner marks an accepted viewing as completed.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int requestId)
        {
            // Step A: Get the logged-in PropertyOwner ID.
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Unauthorized();
            }

            // Step B: Validate the viewing request ID.
            if (requestId <= 0)
            {
                return BadRequest(
                    "A valid viewing request ID is required.");
            }

            try
            {
                // Step C: Complete the viewing request.
                // Facade verifies:
                // - PropertyOwner ownership
                // - Accepted request status
                // - Scheduled viewing time has arrived
                await _viewingRequestFacade.CompleteAsync(
                    requestId,
                    ownerId);

                // Step D: Store a success notification.
                TempData["SuccessMessage"] =
                    "Viewing request marked as completed successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                // Do not expose another owner's requests.
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                // Request may not be Accepted,
                // or the scheduled time has not arrived.
                TempData["ErrorMessage"] = ex.Message;
            }

            // Step E: Return to received viewing requests.
            return RedirectToAction(nameof(Index));
        }

    }
}
