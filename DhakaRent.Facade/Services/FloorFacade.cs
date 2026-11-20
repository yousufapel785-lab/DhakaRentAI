
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;

namespace DhakaRent.Facade.Services
{
    public class FloorFacade : IFloorFacade
    {
        private readonly IFloorRepository _floorRepository;
        private readonly IBuildingRepository _buildingRepository;

        public FloorFacade(
            IFloorRepository floorRepository,
            IBuildingRepository buildingRepository)
        {
            _floorRepository = floorRepository;
            _buildingRepository = buildingRepository;
        }

        // Get floors belonging to an owner's building
        public async Task<IReadOnlyList<Floor>> GetByBuildingAsync(
            int buildingId,
            string ownerId)
        {
            if (buildingId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return new List<Floor>();
            }

            // Verify building ownership
            var building = await _buildingRepository
                .GetByIdAndOwnerAsync(buildingId, ownerId);

            if (building == null)
            {
                return new List<Floor>();
            }

            var floors = await _floorRepository
                .GetByBuildingIdAsync(buildingId, ownerId);

            return floors.ToList();
        }

        // Get one floor with ownership verification
        public async Task<Floor?> GetMineByIdAsync(
            int floorId,
            string ownerId)
        {
            if (floorId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return null;
            }

            return await _floorRepository
                .GetByIdAndOwnerAsync(floorId, ownerId);
        }

        // Create a floor
        public async Task<int> CreateAsync(
            int buildingId,
            string ownerId,
            FloorInput input)
        {
            if (buildingId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                throw new ArgumentException(
                    "Invalid building or owner.");
            }

            ValidateFloorInput(input);

            // Step 1: Verify building ownership
            var building = await _buildingRepository
                .GetByIdAndOwnerAsync(buildingId, ownerId);

            if (building == null || building.IsArchived)
            {
                throw new ArgumentException(
                    "Building not found or unavailable.");
            }

            // Step 2: Prevent duplicate floor numbers
            bool exists = await _floorRepository
                .FloorNumberExistsAsync(
                    buildingId,
                    input.FloorNumber);

            if (exists)
            {
                throw new ArgumentException(
                    "This floor number already exists in the building.");
            }

            // Step 3: Create floor entity
            var floor = new Floor
            {
                BuildingId = buildingId,
                FloorNumber = input.FloorNumber,
                Label = input.Label.Trim()
            };

            // Step 4: Save to database
            await _floorRepository.AddAsync(floor);
            await _floorRepository.SaveChangesAsync();

            return floor.Id;
        }

        // Update an existing floor
        public async Task<bool> UpdateAsync(
            int floorId,
            string ownerId,
            FloorInput input)
        {
            if (floorId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return false;
            }

            ValidateFloorInput(input);

            // Step 1: Verify floor ownership
            var floor = await _floorRepository
                .GetByIdAndOwnerAsync(floorId, ownerId);

            if (floor == null)
            {
                return false;
            }

            // Step 2: Check whether building is active
            var building = await _buildingRepository
                .GetByIdAndOwnerAsync(
                    floor.BuildingId,
                    ownerId);

            if (building == null || building.IsArchived)
            {
                return false;
            }

            // Step 3: Check duplicate number
            // Exclude this floor from the check
            bool exists = await _floorRepository
                .FloorNumberExistsAsync(
                    floor.BuildingId,
                    input.FloorNumber,
                    floor.Id);

            if (exists)
            {
                throw new ArgumentException(
                    "This floor number already exists in the building.");
            }

            // Step 4: Update allowed fields
            floor.FloorNumber = input.FloorNumber;
            floor.Label = input.Label.Trim();

            _floorRepository.Update(floor);
            await _floorRepository.SaveChangesAsync();

            return true;
        }

        // Shared validation for Create and Edit
        private static void ValidateFloorInput(FloorInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            if (input.Label == null ||
                input.Label.Trim().Length > 50)
            {
                throw new ArgumentException(
                    "Floor label cannot exceed 50 characters.");
            }
        }
    }
}
