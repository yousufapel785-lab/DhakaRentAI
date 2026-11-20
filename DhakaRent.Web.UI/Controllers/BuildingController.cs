
using DhakaRent.Facade.Interfaces;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "PropertyOwner")]
    public class BuildingController : Controller
    {
        private readonly IBuildingFacade _buildingFacade;

        public BuildingController(
            IBuildingFacade buildingFacade)
        {
            _buildingFacade = buildingFacade;
        }

        // GET: /Building
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Challenge();
            }

            var buildings = await _buildingFacade
                .GetMineAsync(ownerId);

            return View(buildings);
        }

        // GET: /Building/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new BuildingFormViewModel());
        }

        // POST: /Building/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            BuildingFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Challenge();
            }

            var input = new BuildingInput(
                model.Name,
                model.Address,
                model.Area,
                model.Latitude,
                model.Longitude
            );

            try
            {
                await _buildingFacade.CreateAsync(
                    ownerId, input);

                TempData["SuccessMessage"] =
                    "Building created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);

                return View(model);
            }
        }

        // GET: /Building/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Challenge();
            }

            var building = await _buildingFacade
                .GetMineByIdAsync(id, ownerId);

            if (building == null || building.IsArchived)
            {
                return NotFound();
            }

            var model = new BuildingFormViewModel
            {
                Name = building.Name,
                Address = building.Address,
                Area = building.Area,
                Latitude = building.Latitude,
                Longitude = building.Longitude
            };

            return View(model);
        }

        // POST: /Building/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            BuildingFormViewModel model)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Challenge();
            }

            var input = new BuildingInput(
                model.Name,
                model.Address,
                model.Area,
                model.Latitude,
                model.Longitude
            );

            try
            {
                var updated = await _buildingFacade
                    .UpdateAsync(id, ownerId, input);

                if (!updated)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] =
                    "Building updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);

                return View(model);
            }
        }

        // POST: /Building/Archive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Challenge();
            }

            var archived = await _buildingFacade
                .ArchiveAsync(id, ownerId);

            if (!archived)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Building archived successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
