
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;

namespace DhakaRent.Facade.Services
{
    public class BuildingFacade : IBuildingFacade
    {
        private readonly IBuildingRepository _buildingRepository;

        public BuildingFacade(
            IBuildingRepository buildingRepository)
        {
            _buildingRepository = buildingRepository;
        }

        // Get all buildings of the current owner
        public async Task<IReadOnlyList<Building>> GetMineAsync(
            string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return new List<Building>();
            }

            var buildings = await _buildingRepository
                .GetByOwnerIdAsync(ownerId);

            return buildings.ToList();
        }

        // Get a building only if owned by this user
        public async Task<Building?> GetMineByIdAsync(
            int buildingId,
            string ownerId)
        {
            if (buildingId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return null;
            }

            return await _buildingRepository
                .GetByIdAndOwnerAsync(buildingId, ownerId);
        }

        // Create a new building
        public async Task<int> CreateAsync(
            string ownerId,
            BuildingInput input)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                throw new ArgumentException(
                    "A valid owner is required.");
            }

            ValidateBuildingInput(input);

            var building = new Building
            {
                OwnerId = ownerId,
                Name = input.Name.Trim(),
                Address = input.Address.Trim(),
                Area = input.Area.Trim(),
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                IsArchived = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _buildingRepository.AddAsync(building);
            await _buildingRepository.SaveChangesAsync();

            return building.Id;
        }

        // Update a building owned by this user
        public async Task<bool> UpdateAsync(
            int buildingId,
            string ownerId,
            BuildingInput input)
        {
            if (buildingId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return false;
            }

            var building = await _buildingRepository
                .GetByIdAndOwnerAsync(buildingId, ownerId);

            if (building == null || building.IsArchived)
            {
                return false;
            }

            ValidateBuildingInput(input);

            building.Name = input.Name.Trim();
            building.Address = input.Address.Trim();
            building.Area = input.Area.Trim();
            building.Latitude = input.Latitude;
            building.Longitude = input.Longitude;

            _buildingRepository.Update(building);
            await _buildingRepository.SaveChangesAsync();

            return true;
        }

        // Archive instead of permanently deleting
        public async Task<bool> ArchiveAsync(
            int buildingId,
            string ownerId)
        {
            if (buildingId <= 0 ||
                string.IsNullOrWhiteSpace(ownerId))
            {
                return false;
            }

            var building = await _buildingRepository
                .GetByIdAndOwnerAsync(buildingId, ownerId);

            if (building == null || building.IsArchived)
            {
                return false;
            }

            // Soft archive: preserve the database record
            building.IsArchived = true;

            _buildingRepository.Update(building);
            await _buildingRepository.SaveChangesAsync();

            return true;
        }

        // Shared validation for Create and Edit
        private static void ValidateBuildingInput(
            BuildingInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            if (string.IsNullOrWhiteSpace(input.Name) ||
                input.Name.Trim().Length > 150)
            {
                throw new ArgumentException(
                    "Building name must be 1 to 150 characters.");
            }

            if (string.IsNullOrWhiteSpace(input.Address) ||
                input.Address.Trim().Length > 300)
            {
                throw new ArgumentException(
                    "Address must be 1 to 300 characters.");
            }

            if (string.IsNullOrWhiteSpace(input.Area) ||
                input.Area.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "Area must be 1 to 100 characters.");
            }

            // Coordinates must be supplied together
            if (input.Latitude.HasValue !=
                input.Longitude.HasValue)
            {
                throw new ArgumentException(
                    "Latitude and longitude must both be provided.");
            }

            if (input.Latitude.HasValue &&
                (input.Latitude.Value < -90 ||
                 input.Latitude.Value > 90))
            {
                throw new ArgumentException(
                    "Latitude must be between -90 and 90.");
            }

            if (input.Longitude.HasValue &&
                (input.Longitude.Value < -180 ||
                 input.Longitude.Value > 180))
            {
                throw new ArgumentException(
                    "Longitude must be between -180 and 180.");
            }
        }
    }
}
