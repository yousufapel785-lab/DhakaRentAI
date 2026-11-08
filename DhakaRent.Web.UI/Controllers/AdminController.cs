
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DhakaRent.Entities.Enums;
using DhakaRent.DataAccess.Identity;
using DhakaRent.Facade.Interfaces;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Identity;
namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
        private readonly IPropertyFacade _propertyFacade;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
    IPropertyFacade propertyFacade,
    UserManager<ApplicationUser> userManager)
        {
            _propertyFacade = propertyFacade;
            _userManager = userManager;
        }

        [HttpGet]
        
        public async Task<IActionResult> PendingProperties()
        {
            // Retrieve only pending properties
            var properties = await _propertyFacade
                .GetPropertiesByStatusAsync(ListingStatus.Pending);

            var model = new List<AdminPendingPropertyViewModel>();

            foreach (var property in properties)
            {
                // Retrieve property owner's Identity account
                var owner = await _userManager
                    .FindByIdAsync(property.OwnerId);

                // Convert database entity into ViewModel
                model.Add(new AdminPendingPropertyViewModel
                {
                    Id = property.Id,
                    Title = property.Title,
                    PropertyType = property.PropertyType.ToString(),
                    OwnerName = owner?.FullName ?? "Unknown Owner",
                    Area = property.Area,
                    Address = property.Address,
                    MonthlyRent = property.MonthlyRent,
                    SizeInSqFt = property.SizeInSqFt,
                    Bedrooms = property.Bedrooms,
                    Bathrooms = property.Bathrooms,
                    CreatedAt = property.CreatedAt
                });
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ReviewProperty(int id)
        {
            // Retrieve property using its ID
            var property = await _propertyFacade
                .GetPropertyByIdAsync(id);

            // Check whether property exists
            if (property == null)
            {
                return NotFound();
            }

            // Only pending properties can enter the approval review
            if (property.ListingStatus != ListingStatus.Pending)
            {
                return NotFound();
            }

            // Retrieve property owner's account
            var owner = await _userManager
                .FindByIdAsync(property.OwnerId);

            // Map database entity to ViewModel
            var model = new AdminPropertyReviewViewModel
            {
                Id = property.Id,
                Title = property.Title,
                PropertyType = property.PropertyType.ToString(),
                Description = property.Description,

                Area = property.Area,
                Address = property.Address,

                SizeInSqFt = property.SizeInSqFt,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                IsFurnished = property.IsFurnished,

                MonthlyRent = property.MonthlyRent,
                AvailableFrom = property.AvailableFrom,

                OwnerName = owner?.FullName ?? "Unknown Owner",
                OwnerEmail = owner?.Email ?? "Not Available",

                ListingStatus = property.ListingStatus.ToString(),
                CreatedAt = property.CreatedAt,
                RejectionReason = property.RejectionReason
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            var result = await _propertyFacade.ApprovePropertyAsync(id);

            if (!result)
            {
                TempData["ErrorMessage"] =
                    "Unable to approve this property. It may no longer be pending.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    "Property approved successfully.";
            }

            return RedirectToAction(nameof(PendingProperties));
        }
        [HttpGet]
        public async Task<IActionResult> RejectProperty(int id)
        {
            var property = await _propertyFacade.GetPropertyByIdAsync(id);

            if (property == null ||
                property.ListingStatus != ListingStatus.Pending)
            {
                return NotFound();
            }

            var model = new RejectPropertyViewModel
            {
                PropertyId = property.Id
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProperty(
    RejectPropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _propertyFacade.RejectPropertyAsync(
                model.PropertyId,
                model.Reason);

            if (!result)
            {
                TempData["ErrorMessage"] =
                    "Unable to reject this property. It may no longer be pending.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    "Property rejected successfully.";
            }

            return RedirectToAction(nameof(PendingProperties));
        }
    }

}
