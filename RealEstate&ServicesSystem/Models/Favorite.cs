namespace RealEstate_ServicesSystem.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public Applicationuser? ApplicationUser { get; set; }
        public int ListingId { get; set; }
        public Listing? Listing { get; set; }
    }
}
