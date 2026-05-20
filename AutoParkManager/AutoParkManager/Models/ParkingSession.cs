using System;
using System.ComponentModel.DataAnnotations;

namespace AutoParkManager.Models
{
    public class ParkingSession
    {
        [Key]
        public int SessionId { get; set; }
        public string CarNumber { get; set; } = string.Empty;
        public string CarBrand { get; set; } = string.Empty;
        public int? ClientId { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public decimal? TotalCost { get; set; }
        public bool IsPaid { get; set; }

        public virtual Client Client { get; set; }
    }
}