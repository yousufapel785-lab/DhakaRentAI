
using System.Security.Claims;
using DhakaRent.Facade.Interfaces;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "Tenant")]
    public class SavedPropertyController : Controller
    {
        private readonly ISavedPropertyFacade _savedPropertyFacade;

        public SavedPropertyController(
            ISavedPropertyFacade savedPropertyFacade)
        {
            _savedPropertyFacade = savedPropertyFacade;
        }

        // GET: /SavedProperty
        // Show logged-in Tenant's saved properties.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? tenantId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Unauthorized();
            }

            // Retrieve this Tenant's saved property cards.
            var cards = await _savedPropertyFacade
                .GetMineCardsAsync(tenantId);

            // Convert Facade DTOs to Web UI ViewModels.
            var model = cards.Select(card =>
                new SavedPropertyCardViewModel
                {
                    PropertyId = card.PropertyId,
                    SavedAt = card.SavedAt,
                    Title = card.Title,
                    Area = card.Area,
                    Address = card.Address,
                    MonthlyRent = card.MonthlyRent,
                    Bedrooms = card.Bedrooms,
                    Bathrooms = card.Bathrooms,
                    ImageUrl = card.ImageUrl
                })
                .ToList();

            return View(model);
        }

        // POST: /SavedProperty/Save
        // Save Property to Wishlist.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            int propertyId,
            bool returnToDetails = false,
            string? returnUrl = null)
        {
            string? tenantId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Unauthorized();
            }

            try
            {
                bool saved = await _savedPropertyFacade
                    .SaveAsync(tenantId, propertyId);

                if (saved)
                {
                    TempData["SuccessMessage"] =
                        "Property added to your wishlist successfully.";
                }
                else
                {
                    TempData["InfoMessage"] =
                        "This property is already in your wishlist.";
                }
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            // Redirect according to the originating page.
            return RedirectAfterWishlistAction(
                propertyId,
                returnToDetails,
                returnUrl);
        }

        // POST: /SavedProperty/Remove
        // Remove Property from Wishlist.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int propertyId,
            bool returnToDetails = false,
            string? returnUrl = null)
        {
            string? tenantId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Unauthorized();
            }

            bool removed = await _savedPropertyFacade
                .RemoveAsync(tenantId, propertyId);

            if (removed)
            {
                TempData["SuccessMessage"] =
                    "Property removed from your wishlist.";
            }
            else
            {
                TempData["InfoMessage"] =
                    "Property was not found in your wishlist.";
            }

            // Redirect according to the originating page.
            return RedirectAfterWishlistAction(
                propertyId,
                returnToDetails,
                returnUrl);
        }

        // Handles safe redirects after Save or Remove.
        // Supports:
        // 1. Property Details
        // 2. Browse Properties with search filters
        // 3. My Wishlist (default)
        private IActionResult RedirectAfterWishlistAction(
            int propertyId,
            bool returnToDetails,
            string? returnUrl)
        {
            // Priority 1:
            // Return to the same Property Details page.
            if (returnToDetails && propertyId > 0)
            {
                return RedirectToAction(
                    "Details",
                    "Properties",
                    new { id = propertyId });
            }

            // Priority 2:
            // Return to Browse Properties.
            // Accept only local Browse Properties URLs
            // to prevent unsafe external redirects.
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                bool isBrowsePropertiesUrl =
                    returnUrl.Equals(
                        "/Properties",
                        StringComparison.OrdinalIgnoreCase)

                    || returnUrl.StartsWith(
                        "/Properties?",
                        StringComparison.OrdinalIgnoreCase)

                    || returnUrl.Equals(
                        "/Properties/Index",
                        StringComparison.OrdinalIgnoreCase)

                    || returnUrl.StartsWith(
                        "/Properties/Index?",
                        StringComparison.OrdinalIgnoreCase);

                if (isBrowsePropertiesUrl)
                {
                    return LocalRedirect(returnUrl);
                }
            }

            // Priority 3:
            // Default behavior for My Wishlist page.
            return RedirectToAction(nameof(Index));
        }
    }
}
