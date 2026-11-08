using System.ComponentModel.DataAnnotations;

namespace DhakaRent.Entities.Models
{
    public class RentPredictionHistory
    {
        // Primary Key
        public int Id { get; set; }

        // Tenant Information
        [Required]
        public string TenantId { get; set; } = string.Empty;

        // Prediction Input Snapshot
        [Required]
        public string InputDataJson { get; set; } = string.Empty;

        // Prediction Result (BDT)
        [Range(0, 100000000)]
        public decimal PredictedRent { get; set; }

        // Model Information
        [StringLength(100)]
        public string? ModelVersion { get; set; }

        // Prediction Timestamp
        public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
    }
}