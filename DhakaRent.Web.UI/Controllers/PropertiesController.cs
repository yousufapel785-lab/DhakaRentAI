
using System.Security.Claims;
using DhakaRent.Entities.Enums;
using DhakaRent.Facade.Interfaces;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhakaRent.Web.UI.Controllers
{
    [AllowAnonymous]
    public class PropertiesController : Controller
    {
        private readonly IPropertyFacade _propertyFacade;
        private readonly ISavedPropertyFacade _savedPropertyFacade;

        public PropertiesController(
            IPropertyFacade propertyFacade,
            ISavedPropertyFacade savedPropertyFacade)
        {
            _propertyFacade = propertyFacade;
            _savedPropertyFacade = savedPropertyFacade;
        }

        // GET: /Properties
        // Search and display approved properties.

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] PublicPropertySearchViewModel search)
        {
            // Step 1: Validate search parameters.
            if (search.MinRent is < 0)
            {
                ModelState.AddModelError(
                    nameof(search.MinRent),
                    "Minimum rent cannot be negative.");
            }

            if (search.MaxRent is < 0)
            {
                ModelState.AddModelError(
                    nameof(search.MaxRent),
                    "Maximum rent cannot be negative.");
            }

            if (search.MinRent.HasValue &&
                search.MaxRent.HasValue &&
                search.MinRent.Value > search.MaxRent.Value)
            {
                ModelState.AddModelError(
                    nameof(search.MaxRent),
                    "Maximum rent must be greater than or equal to minimum rent.");
            }

            if (search.Bedrooms is < 0)
            {
                ModelState.AddModelError(
                    nameof(search.Bedrooms),
                    "Bedrooms cannot be negative.");
            }

            if (search.PropertyType.HasValue &&
                !Enum.IsDefined(search.PropertyType.Value))
            {
                ModelState.AddModelError(
                    nameof(search.PropertyType),
                    "Invalid property type.");
            }

            // Step 2: Stop if search parameters are invalid.
            if (!ModelState.IsValid)
            {
                return View(search);
            }

            // Step 3: Retrieve filtered approved properties.
            var properties = await _propertyFacade
                .SearchApprovedPropertiesAsync(
                    search.Area,
                    search.PropertyType,
                    search.MinRent,
                    search.MaxRent,
                    search.Bedrooms);

            // Step 4: Prepare an empty Wishlist ID set.
            // Guests and PropertyOwners will keep this empty.
            var savedPropertyIds = new HashSet<int>();

            // Step 5: Retrieve saved properties
            // only for an authenticated Tenant.
            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Tenant"))
            {
                string? tenantId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    var savedProperties = await _savedPropertyFacade
                        .GetMineCardsAsync(tenantId);

                    // Store saved Property IDs for fast lookup.
                    savedPropertyIds = savedProperties
                        .Select(saved => saved.PropertyId)
                        .ToHashSet();
                }
            }

            // Step 6: Map properties to public ViewModels
            // and include actual Wishlist Saved State.
            search.Properties = properties.Select(property =>
                new PublicPropertyListItemViewModel
                {
                    Id = property.Id,
                    Title = property.Title,
                    Area = property.Area,
                    Address = property.Address,
                    PropertyType = property.PropertyType.ToString(),
                    MonthlyRent = property.MonthlyRent,
                    SizeInSqFt = property.SizeInSqFt,
                    Bedrooms = property.Bedrooms,
                    Bathrooms = property.Bathrooms,
                    IsFurnished = property.IsFurnished,

                    // NEW: True only when this Tenant
                    // has saved the current property.
                    IsSavedToWishlist =
                        savedPropertyIds.Contains(property.Id)
                })
                .ToList();

            // Step 7: Return search results.
            return View(search);
        }


        // GET: /Properties/Details/1
        // Display approved property details.
        // Also check if the logged-in Tenant
        // has saved this property to their wishlist.
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Step A: Validate Property ID.
            if (id <= 0)
            {
                return NotFound();
            }

            // Step B: Retrieve property information.
            var property = await _propertyFacade
                .GetPropertyByIdAsync(id);

            // Step C: Only approved properties
            // are visible on the public details page.
            if (property == null ||
                property.ListingStatus != ListingStatus.Approved)
            {
                return NotFound();
            }

            // Step D: Default wishlist state.
            // Guests and non-Tenant users are not checked.
            bool isSavedToWishlist = false;

            // Step E: Check wishlist only for
            // authenticated Tenant users.
            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Tenant"))
            {
                // Get logged-in Tenant ID.
                string? tenantId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    // Retrieve only this Tenant's
                    // saved property cards.
                    var savedProperties = await _savedPropertyFacade
                        .GetMineCardsAsync(tenantId);

                    // Check if this Property ID exists
                    // in the Tenant's saved properties.
                    isSavedToWishlist = savedProperties
                        .Any(saved => saved.PropertyId == property.Id);
                }
            }

            // Step F: Prepare property details ViewModel.
            var model = new PublicPropertyDetailsViewModel
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                PropertyType = property.PropertyType.ToString(),
                Area = property.Area,
                Address = property.Address,
                SizeInSqFt = property.SizeInSqFt,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                IsFurnished = property.IsFurnished,
                MonthlyRent = property.MonthlyRent,
                AvailableFrom = property.AvailableFrom,

                // NEW: Actual wishlist state
                // of the logged-in Tenant.
                IsSavedToWishlist = isSavedToWishlist
            };

            // Step G: Return property details.
            return View(model);
        }
    }
}
