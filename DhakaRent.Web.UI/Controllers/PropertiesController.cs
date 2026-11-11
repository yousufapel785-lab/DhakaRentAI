
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

        public PropertiesController(IPropertyFacade propertyFacade)
        {
            _propertyFacade = propertyFacade;
        }


        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] PublicPropertySearchViewModel search)
        {
            // Step 1: Validate search parameters
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

            // Step 2: Stop search if input is invalid
            if (!ModelState.IsValid)
            {
                return View(search);
            }

            // Step 3: Get filtered approved properties
            var properties = await _propertyFacade
                .SearchApprovedPropertiesAsync(
                    search.Area,
                    search.PropertyType,
                    search.MinRent,
                    search.MaxRent,
                    search.Bedrooms);

            // Step 4: Map entities to public ViewModels
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
                    IsFurnished = property.IsFurnished
                }).ToList();

            // Step 5: Return search filters and results
            return View(search);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var property = await _propertyFacade.GetPropertyByIdAsync(id);

            if (property == null ||
                property.ListingStatus != ListingStatus.Approved)
            {
                return NotFound();
            }

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
                AvailableFrom = property.AvailableFrom
            };

            return View(model);
        }
  
    }
}
