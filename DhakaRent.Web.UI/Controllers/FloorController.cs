
using DhakaRent.Facade.Interfaces;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "PropertyOwner")]
    public class FloorController : Controller
    {
        private readonly IFloorFacade _floorFacade;
        private readonly IBuildingFacade _buildingFacade;

        public FloorController(
            IFloorFacade floorFacade,
            IBuildingFacade buildingFacade)
        {
            _floorFacade = floorFacade;
            _buildingFacade = buildingFacade;
        }

        // GET: /Floor?buildingId=2
        [HttpGet]
        public async Task<IActionResult> Index(int buildingId)
        {
            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
                return Challenge();

            // Verify building belongs to logged-in owner
            var building = await _buildingFacade
                .GetMineByIdAsync(buildingId, ownerId);

            if (building == null)
                return NotFound();

            var floors = await _floorFacade
                .GetByBuildingAsync(buildingId, ownerId);

            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;
            ViewBag.IsArchived = building.IsArchived;

            return View(floors);
        }

        // GET: /Floor/Create?buildingId=2
        [HttpGet]
        public async Task<IActionResult> Create(int buildingId)
        {
            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
                return Challenge();

            var building = await _buildingFacade
                .GetMineByIdAsync(buildingId, ownerId);

            // Cannot create floors in archived buildings
            if (building == null || building.IsArchived)
                return NotFound();

            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;

            return View(new FloorFormViewModel());
        }

        // POST: /Floor/Create?buildingId=2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int buildingId,
            FloorFormViewModel model)
        {
            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
                return Challenge();

            var building = await _buildingFacade
                .GetMineByIdAsync(buildingId, ownerId);

            if (building == null || building.IsArchived)
                return NotFound();

            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;

            if (!ModelState.IsValid)
                return View(model);

            var input = new FloorInput(
                model.FloorNumber,
                model.Label
            );

            try
            {
                await _floorFacade.CreateAsync(
                    buildingId, ownerId, input);

                TempData["SuccessMessage"] =
                    "Floor created successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { buildingId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);

                return View(model);
            }
        }

        // GET: /Floor/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
                return Challenge();

            var floor = await _floorFacade
                .GetMineByIdAsync(id, ownerId);

            if (floor == null)
                return NotFound();

            var building = await _buildingFacade
                .GetMineByIdAsync(floor.BuildingId, ownerId);

            if (building == null || building.IsArchived)
                return NotFound();

            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;

            var model = new FloorFormViewModel
            {
                FloorNumber = floor.FloorNumber,
                Label = floor.Label
            };

            return View(model);
        }

        // POST: /Floor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            FloorFormViewModel model)
        {
            var ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
                return Challenge();

            var floor = await _floorFacade
                .GetMineByIdAsync(id, ownerId);

            if (floor == null)
                return NotFound();

            var building = await _buildingFacade
                .GetMineByIdAsync(floor.BuildingId, ownerId);

            if (building == null || building.IsArchived)
                return NotFound();

            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;

            if (!ModelState.IsValid)
                return View(model);

            var input = new FloorInput(
                model.FloorNumber,
                model.Label
            );

            try
            {
                var updated = await _floorFacade
                    .UpdateAsync(id, ownerId, input);

                if (!updated)
                    return NotFound();

                TempData["SuccessMessage"] =
                    "Floor updated successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { buildingId = floor.BuildingId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);

                return View(model);
            }
        }
    }
}
