using RealEstate_ServicesSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class EditListingmodel
    {
        public int Id { get; set; }
        public int? UnitId { get; set; }
        public List<Unit?> Units { get; set; }
        public DateTime? ListedAt { get; set; }

        public decimal? BrokerPercentage { get; set; }
        public bool IsActive { get; set; }
        public RentPeriod? Rentperiod { get; set; }

        public string? Description { get; set; } = string.Empty;
        public DateTime createAt { get; set; } = DateTime.Now;
        public Listing? Listing { get; set; }
    }
}
