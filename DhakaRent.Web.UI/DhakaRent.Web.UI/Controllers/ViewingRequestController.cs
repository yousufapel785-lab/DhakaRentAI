
using System.Security.Claims;
using DhakaRent.Facade.Interfaces;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "Tenant")]
    public class ViewingRequestController : Controller
    {
        private readonly IViewingRequestFacade
            _viewingRequestFacade;

        public ViewingRequestController(
            IViewingRequestFacade viewingRequestFacade)
        {
            _viewingRequestFacade = viewingRequestFacade;
        }

        // GET: /ViewingRequest/Create?propertyId=10
        [HttpGet]
        public IActionResult Create(int propertyId)
        {
            if (propertyId <= 0)
            {
                return BadRequest(
                    "A valid property ID is required.");
            }

            var model = new ViewingRequestFormViewModel
            {
                PropertyId = propertyId
            };

            return View(model);
        }

        // POST: /ViewingRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ViewingRequestFormViewModel model)
        {
            // Step A: Get the logged-in Tenant ID.
            string? tenantId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Unauthorized();
            }

            // Step B: Validate the submitted form.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Step C: Ensure date/time was provided.
            if (!model.RequestedDateTimeLocal.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.RequestedDateTimeLocal),
                    "Please select a viewing date and time.");

                return View(model);
            }

            try
            {
                // Step D: Get the Bangladesh time zone.
                // Windows and Linux use different time zone IDs.
                string timeZoneId = OperatingSystem.IsWindows()
                    ? "Bangladesh Standard Time"
                    : "Asia/Dhaka";

                TimeZoneInfo bangladeshTimeZone =
                    TimeZoneInfo.FindSystemTimeZoneById(
                        timeZoneId);

                // Step E: Treat form input as Bangladesh
                // local time, not server local time.
                DateTime localDateTime =
                    DateTime.SpecifyKind(
                        model.RequestedDateTimeLocal.Value,
                        DateTimeKind.Unspecified);

                // Step F: Convert Bangladesh time to UTC.
                DateTime requestedDateTimeUtc =
                    TimeZoneInfo.ConvertTimeToUtc(
                        localDateTime,
                        bangladeshTimeZone);

                // Step G: Validate future viewing date/time.
                if (requestedDateTimeUtc <= DateTime.UtcNow)
                {
                    ModelState.AddModelError(
                        nameof(model.RequestedDateTimeLocal),
                        "Please select a future viewing date and time.");

                    return View(model);
                }

                // Step H: Create viewing request using Facade.
                await _viewingRequestFacade.CreateAsync(
                    model.PropertyId,
                    tenantId,
                    requestedDateTimeUtc,
                    model.TenantMessage);

                // Step I: Store success notification.
                TempData["SuccessMessage"] =
                    "Your viewing request has been submitted successfully.";

                // Step J: Redirect after successful submission.
                return RedirectToAction(
                    "Dashboard",
                    "Tenant");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
            }

            // Show validation/business errors in the form.
            return View(model);
        }


        // GET: /ViewingRequest/Index
        // Tenant sees their own requests with property details.
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

            // Step B: Retrieve only this Tenant's requests.
            var requests = await _viewingRequestFacade
                .GetMineAsync(tenantId);

            // Step C: Collect unique Property IDs.
            int[] propertyIds = requests
                .Select(r => r.PropertyId)
                .Distinct()
                .ToArray();

            // Step D: Fetch property details in bulk.
            var properties = await _viewingRequestFacade
                .GetPropertiesByIdsAsync(propertyIds);

            // Step E: Combine each request with its property.
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

            // Step F: Prepare enriched data for the View.
            ViewData["ViewingRequestItems"] = viewModels;

            // Step G: Keep the existing View Model working.
            return View(requests);
        }


        // POST: /ViewingRequest/Cancel
        // Tenant can cancel only their own viewing request.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int requestId)
        {
            // Step A: Get the logged-in Tenant ID.
            string? tenantId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(tenantId))
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
                // Step C: Cancel using the Facade.
                // The Facade verifies Tenant ownership
                // and allowed request statuses.
                await _viewingRequestFacade.CancelAsync(
                    requestId,
                    tenantId);

                // Step D: Store success notification.
                TempData["SuccessMessage"] =
                    "Your viewing request has been cancelled successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                // Do not expose requests belonging to
                // another Tenant.
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                // Request may already be cancelled,
                // rejected, or completed.
                TempData["ErrorMessage"] = ex.Message;
            }

            // Step E: Return to My Viewing Requests.
            return RedirectToAction(nameof(Index));
        }

        // POST: /ViewingRequest/AcceptProposedTime
        // Tenant accepts the owner's proposed viewing time.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptProposedTime(
            int requestId)
        {
            // Step A: Get the logged-in Tenant ID.
            string? tenantId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(tenantId))
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
                // Step C: Accept the owner's proposed time.
                // Facade validates ownership, status,
                // and whether the proposed time is in the future.
                await _viewingRequestFacade
                    .AcceptProposedTimeAsync(
                        requestId,
                        tenantId);

                // Step D: Store success notification.
                TempData["SuccessMessage"] =
                    "You have accepted the proposed viewing time successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                // Do not expose another Tenant's requests.
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                // Invalid status, missing proposed time,
                // or proposed time already passed.
                TempData["ErrorMessage"] = ex.Message;
            }

            // Step E: Return to My Viewing Requests.
            return RedirectToAction(nameof(Index));
        }


    }
}
