
using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Entities.Models
{
    public class Floor
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Key - Building
        public int BuildingId { get; set; }

        // Floor Information
        public int FloorNumber { get; set; }

        [StringLength(50)]
        public string Label { get; set; } = string.Empty;

        // Navigation Property
        public Building Building { get; set; } = null!;

        // One Floor can have multiple Rental Units
        public ICollection<RentalUnit> Units { get; set; }
            = new List<RentalUnit>();

    }
}
