using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstate_ServicesSystem.Models
{
   
    public enum ListingType
    {
        Sale,
        Rent
    }
   public enum RentPeriod
    {
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
    public class Listing
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public Unit? Unit { get; set; }
        public DateTime ListedAt { get; set; }
        public decimal Price { get; set; }
        public RentPeriod? Rentperiod { get; set; }
        [Column("BrokerPercentage")]
        public decimal? BrokerPercentage { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public ListingType? listingType { get; set; }
        public DateTime createAt { get; set; } = DateTime.Now;
       
        public string ApplicationUserId { get; set; }
        public Applicationuser? ApplicationUser { get; set; }

    }
}
