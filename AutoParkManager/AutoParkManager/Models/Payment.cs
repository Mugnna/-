using System;
using System.ComponentModel.DataAnnotations;

namespace AutoParkManager.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        public int SessionId { get; set; }
        public string CarNumber { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }
        public decimal DurationHours { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}