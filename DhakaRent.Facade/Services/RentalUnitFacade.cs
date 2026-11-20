
using DhakaRent.Entities.Enums;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;

namespace DhakaRent.Facade.Services
{
    public class RentalUnitFacade : IRentalUnitFacade
    {
        private readonly IRentalUnitRepository _unitRepository;
        private readonly IFloorRepository _floorRepository;
        private readonly IBuildingRepository _buildingRepository;

        public RentalUnitFacade(
            IRentalUnitRepository unitRepository,
            IFloorRepository floorRepository,
            IBuildingRepository buildingRepository)
        {
            _unitRepository = unitRepository;
            _floorRepository = floorRepository;
            _buildingRepository = buildingRepository;
        }

        // 1. Get units belonging to an owner's floor
        public async Task<IReadOnlyList<RentalUnit>> GetByFloorAsync(
            int floorId,
            string ownerId)
        {
            if (floorId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return new List<RentalUnit>();
            }

            // Verify floor ownership
            var floor = await _floorRepository
                .GetByIdAndOwnerAsync(floorId, ownerId);

            if (floor == null)
            {
                return new List<RentalUnit>();
            }

            var units = await _unitRepository
                .GetByFloorIdAsync(floorId, ownerId);

            return units.ToList();
        }

        // 2. Get one unit with ownership verification
        public async Task<RentalUnit?> GetMineByIdAsync(
            int unitId,
            string ownerId)
        {
            if (unitId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return null;
            }

            return await _unitRepository
                .GetByIdAndOwnerAsync(unitId, ownerId);
        }

        // 3. Create a new rental unit
        public async Task<int> CreateAsync(
            int floorId,
            string ownerId,
            RentalUnitInput input)
        {
            if (floorId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                throw new ArgumentException(
                    "Invalid floor or owner.");
            }

            ValidateUnitInput(input);

            // Step 1: Verify floor ownership
            var floor = await _floorRepository
                .GetByIdAndOwnerAsync(floorId, ownerId);

            if (floor == null)
            {
                throw new ArgumentException(
                    "Floor not found or access denied.");
            }

            // Step 2: Check whether building is active
            var building = await _buildingRepository
                .GetByIdAndOwnerAsync(
                    floor.BuildingId,
                    ownerId);

            if (building == null || building.IsArchived)
            {
                throw new ArgumentException(
                    "Building not found or archived.");
            }

            // Step 3: Prevent duplicate unit numbers
            string unitNumber = input.UnitNumber.Trim();

            bool exists = await _unitRepository
                .UnitNumberExistsAsync(
                    floorId,
                    unitNumber);

            if (exists)
            {
                throw new ArgumentException(
                    "This unit number already exists on this floor.");
            }

            // Step 4: Create unit
            var unit = new RentalUnit
            {
                FloorId = floorId,
                UnitNumber = unitNumber,
                SizeInSqFt = input.SizeInSqFt,
                StandardMonthlyRent = input.StandardMonthlyRent,
                Bedrooms = input.Bedrooms,
                Bathrooms = input.Bathrooms,
                OccupancyStatus = UnitOccupancyStatus.Vacant
            };

            // Step 5: Save to database
            await _unitRepository.AddAsync(unit);
            await _unitRepository.SaveChangesAsync();

            return unit.Id;
        }

        // 4. Update an existing rental unit
        public async Task<bool> UpdateAsync(
            int unitId,
            string ownerId,
            RentalUnitInput input)
        {
            if (unitId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return false;
            }

            ValidateUnitInput(input);

            // Step 1: Verify unit ownership
            var unit = await _unitRepository
                .GetByIdAndOwnerAsync(unitId, ownerId);

            if (unit == null)
            {
                return false;
            }

            // Step 2: Verify floor ownership
            var floor = await _floorRepository
                .GetByIdAndOwnerAsync(
                    unit.FloorId,
                    ownerId);

            if (floor == null)
            {
                return false;
            }

            // Step 3: Prevent editing units in archived buildings
            var building = await _buildingRepository
                .GetByIdAndOwnerAsync(
                    floor.BuildingId,
                    ownerId);

            if (building == null || building.IsArchived)
            {
                return false;
            }

            // Step 4: Check duplicate unit number
            string unitNumber = input.UnitNumber.Trim();

            bool exists = await _unitRepository
                .UnitNumberExistsAsync(
                    unit.FloorId,
                    unitNumber,
                    unit.Id);

            if (exists)
            {
                throw new ArgumentException(
                    "This unit number already exists on this floor.");
            }

            // Step 5: Update allowed fields only
            unit.UnitNumber = unitNumber;
            unit.SizeInSqFt = input.SizeInSqFt;
            unit.StandardMonthlyRent = input.StandardMonthlyRent;
            unit.Bedrooms = input.Bedrooms;
            unit.Bathrooms = input.Bathrooms;

            // OccupancyStatus is intentionally unchanged.

            _unitRepository.Update(unit);
            await _unitRepository.SaveChangesAsync();

            return true;
        }

        // 5. Shared validation for Create and Edit
        private static void ValidateUnitInput(
            RentalUnitInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            if (string.IsNullOrWhiteSpace(input.UnitNumber) ||
                input.UnitNumber.Trim().Length > 30)
            {
                throw new ArgumentException(
                    "Unit number must be 1 to 30 characters.");
            }

            if (input.SizeInSqFt < 1 ||
                input.SizeInSqFt > 100000)
            {
                throw new ArgumentException(
                    "Unit size must be between 1 and 100000 square feet.");
            }

            if (input.StandardMonthlyRent < 0 ||
                input.StandardMonthlyRent > 100000000)
            {
                throw new ArgumentException(
                    "Monthly rent must be between 0 and 100000000.");
            }

            if (input.Bedrooms < 0 || input.Bedrooms > 50)
            {
                throw new ArgumentException(
                    "Bedrooms must be between 0 and 50.");
            }

            if (input.Bathrooms < 0 || input.Bathrooms > 50)
            {
                throw new ArgumentException(
                    "Bathrooms must be between 0 and 50.");
            }
        }
    }
}
