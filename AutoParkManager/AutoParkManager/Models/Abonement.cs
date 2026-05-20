using System.ComponentModel.DataAnnotations;

namespace AutoParkManager.Models
{
    public class Abonement
    {
        [Key]
        public int AbonementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int ValidDays { get; set; }
    }
}