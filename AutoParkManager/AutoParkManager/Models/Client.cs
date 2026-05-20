using System;
using System.ComponentModel.DataAnnotations;

namespace AutoParkManager.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CarNumber { get; set; } = string.Empty;
        public string CarBrand { get; set; } = string.Empty;
        public int? AbonementId { get; set; }
        public DateTime? AbonementStartDate { get; set; }
        public DateTime? AbonementEndDate { get; set; }
    }
}