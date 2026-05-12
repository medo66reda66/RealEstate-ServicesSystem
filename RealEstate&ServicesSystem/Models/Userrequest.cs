namespace RealEstate_ServicesSystem.Models
{
    public enum RequestType
    {
        Visit,
        Buy,
        Rent,
        Inquiry
    }
    public class Userrequest
    {
        public int Id { get; set; }
        public RequestType? RequestType { get; set; }
        public string msg { get; set; }= string.Empty;
        public int ListingId { get; set; }
        public Listing? Listing { get; set; }
        public DateTime ReuquestAt { get; set; } = DateTime.UtcNow;
        public string? ApplicationuserId { get; set; }
        public Applicationuser? Applicationuser { get; set; }
    }
}
