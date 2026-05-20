using System.ComponentModel.DataAnnotations;

namespace AutoParkManager.Models
{
    public class Tariff
    {
        [Key]
        public int TariffId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }
        public bool IsNightTariff { get; set; }
    }
}