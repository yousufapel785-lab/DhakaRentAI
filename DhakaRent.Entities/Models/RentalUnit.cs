
using System.ComponentModel.DataAnnotations;
using DhakaRent.Entities.Enums;

namespace DhakaRent.Entities.Models
{
    public class RentalUnit
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Key - Floor
        public int FloorId { get; set; }

        // Unit Information
        [Required]
        [StringLength(30)]
        public string UnitNumber { get; set; } = string.Empty;

        [Range(1, 100000)]
        public decimal SizeInSqFt { get; set; }

        [Range(0, 100000000)]
        public decimal StandardMonthlyRent { get; set; }

        [Range(0, 50)]
        public int Bedrooms { get; set; }

        [Range(0, 50)]
        public int Bathrooms { get; set; }

        // Occupancy Status
        public UnitOccupancyStatus OccupancyStatus
        {
            get;
            set;
        } = UnitOccupancyStatus.Vacant;

        // Navigation Property
        public Floor Floor { get; set; } = null!;
    }
}
