
using System.Security.Claims;
using DhakaRent.Facade.Interfaces;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "PropertyOwner")]
    public class RentalUnitController : Controller
    {
        private readonly IRentalUnitFacade _unitFacade;
        private readonly IFloorFacade _floorFacade;
        private readonly IBuildingFacade _buildingFacade;

        public RentalUnitController(
            IRentalUnitFacade unitFacade,
            IFloorFacade floorFacade,
            IBuildingFacade buildingFacade)
        {
            _unitFacade = unitFacade;
            _floorFacade = floorFacade;
            _buildingFacade = buildingFacade;
        }

        // GET: /RentalUnit?floorId=1
        [HttpGet]
        public async Task<IActionResult> Index(int floorId)
        {
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
                return Unauthorized();

            var floor = await _floorFacade
                .GetMineByIdAsync(floorId, ownerId);

            if (floor == null)
                return NotFound();

            var building = await _buildingFacade
                .GetMineByIdAsync(floor.BuildingId, ownerId);

            if (building == null)
                return NotFound();

            ViewBag.FloorId = floor.Id;
            ViewBag.FloorNumber = floor.FloorNumber;
            ViewBag.FloorLabel = floor.Label;
            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;
            ViewBag.IsArchived = building.IsArchived;

            var units = await _unitFacade
                .GetByFloorAsync(floorId, ownerId);

            return View(units);
        }

        // GET: /RentalUnit/Create?floorId=1
        [HttpGet]
        public async Task<IActionResult> Create(int floorId)
        {
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
                return Unauthorized();

            var floor = await _floorFacade
                .GetMineByIdAsync(floorId, ownerId);

            if (floor == null)
                return NotFound();

            var building = await _buildingFacade
                .GetMineByIdAsync(floor.BuildingId, ownerId);

            if (building == null || building.IsArchived)
                return NotFound();

            ViewBag.FloorId = floor.Id;
            ViewBag.FloorLabel = floor.Label;
            ViewBag.BuildingName = building.Name;

            return View(new RentalUnitFormViewModel());
        }

        // POST: /RentalUnit/Create?floorId=1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int floorId,
            RentalUnitFormViewModel model)
        {
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
                return Unauthorized();

            var floor = await _floorFacade
                .GetMineByIdAsync(floorId, ownerId);

            if (floor == null)
                return NotFound();

            var building = await _buildingFacade
                .GetMineByIdAsync(floor.BuildingId, ownerId);

            if (building == null || building.IsArchived)
                return NotFound();

            ViewBag.FloorId = floor.Id;
            ViewBag.FloorLabel = floor.Label;
            ViewBag.BuildingName = building.Name;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var input = new RentalUnitInput(
                    model.UnitNumber ?? string.Empty,
                    model.SizeInSqFt,
                    model.StandardMonthlyRent,
                    model.Bedrooms,
                    model.Bathrooms
                );

                await _unitFacade.CreateAsync(
                    floorId,
                    ownerId,
                    input);

                TempData["SuccessMessage"] =
                    "Rental unit created successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { floorId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
        }

        // GET: /RentalUnit/Edit/1
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
                return Unauthorized();

            var unit = await _unitFacade
                .GetMineByIdAsync(id, ownerId);

            if (unit == null)
                return NotFound();

            var floor = await _floorFacade
                .GetMineByIdAsync(unit.FloorId, ownerId);

            if (floor == null)
                return NotFound();

            var building = await _buildingFacade
                .GetMineByIdAsync(floor.BuildingId, ownerId);

            if (building == null || building.IsArchived)
                return NotFound();

            ViewBag.UnitId = unit.Id;
            ViewBag.FloorId = floor.Id;
            ViewBag.FloorLabel = floor.Label;
            ViewBag.BuildingName = building.Name;

            var model = new RentalUnitFormViewModel
            {
                UnitNumber = unit.UnitNumber,
                SizeInSqFt = unit.SizeInSqFt,
                StandardMonthlyRent =
                    unit.StandardMonthlyRent,
                Bedrooms = unit.Bedrooms,
                Bathrooms = unit.Bathrooms
            };

            return View(model);
        }

        // POST: /RentalUnit/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            RentalUnitFormViewModel model)
        {
            string? ownerId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(ownerId))
                return Unauthorized();

            var unit = await _unitFacade
                .GetMineByIdAsync(id, ownerId);

            if (unit == null)
                return NotFound();

            var floor = await _floorFacade
                .GetMineByIdAsync(unit.FloorId, ownerId);

            if (floor == null)
                return NotFound();

            var building = await _buildingFacade
                .GetMineByIdAsync(floor.BuildingId, ownerId);

            if (building == null || building.IsArchived)
                return NotFound();

            ViewBag.UnitId = unit.Id;
            ViewBag.FloorId = floor.Id;
            ViewBag.FloorLabel = floor.Label;
            ViewBag.BuildingName = building.Name;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var input = new RentalUnitInput(
                    model.UnitNumber ?? string.Empty,
                    model.SizeInSqFt,
                    model.StandardMonthlyRent,
                    model.Bedrooms,
                    model.Bathrooms
                );

                bool updated = await _unitFacade.UpdateAsync(
                    id,
                    ownerId,
                    input);

                if (!updated)
                    return NotFound();

                TempData["SuccessMessage"] =
                    "Rental unit updated successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { floorId = floor.Id });
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
