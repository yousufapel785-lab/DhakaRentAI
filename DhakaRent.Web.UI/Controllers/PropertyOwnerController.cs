
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DhakaRent.Entities.Enums;
using System.Linq;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "PropertyOwner")]
    public class PropertyOwnerController : Controller
    {
        private readonly IPropertyFacade _propertyFacade;

        public PropertyOwnerController(
            IPropertyFacade propertyFacade)
        {
            _propertyFacade = propertyFacade;
        }

        // Property Owner Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(ownerId))
            {
                return Forbid();
            }

            // Get properties belonging to the logged-in owner
            var properties = (await _propertyFacade
                .GetPropertiesByOwnerIdAsync(ownerId))
                .ToList();

            // Prepare dashboard data
            var model = new PropertyOwnerDashboardViewModel
            {
                TotalProperties = properties.Count,

                PendingProperties = properties.Count(
                    p => p.ListingStatus == ListingStatus.Pending),

                ApprovedProperties = properties.Count(
                    p => p.ListingStatus == ListingStatus.Approved),

                RejectedProperties = properties.Count(
                    p => p.ListingStatus == ListingStatus.Rejected),

                Properties = properties
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new PropertyListItemViewModel
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Area = p.Area,
                        MonthlyRent = p.MonthlyRent,
                        ListingStatus = p.ListingStatus.ToString(),
                        CreatedAt = p.CreatedAt
                    })
                    .ToList()
            };

            return View(model);
        }

        // Display Add Property Form
        [HttpGet]
        public IActionResult CreateProperty()
        {
            return View();
        }

        // Process Add Property Form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProperty(
            PropertyCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get authenticated owner's ID from Identity
            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(ownerId))
            {
                return Forbid();
            }

            // Map ViewModel to database entity
            var property = new Property
            {
                OwnerId = ownerId,
                Title = model.Title,
                PropertyType = model.PropertyType,
                Description = model.Description,
                Area = model.Area,
                Address = model.Address,
                SizeInSqFt = model.SizeInSqFt,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                IsFurnished = model.IsFurnished,
                MonthlyRent = model.MonthlyRent,
                AvailableFrom = model.AvailableFrom
            };

            await _propertyFacade.CreatePropertyAsync(property);

            TempData["SuccessMessage"] =
                "Your property has been submitted for admin approval.";

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
