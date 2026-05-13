namespace RealEstate_ServicesSystem.Models
{

    public enum UnitType
    {
        Apartment,
        House,
        Condo,
        Townhouse,
        Studio
    }
    public enum UnitPurpose
    {
        Sale,
        Rent
    }
    public enum UnitStatus
    {
        Available,
        Sold,
        Rented
    }
    public class Unit
    {
        public int Id { get; set; }
        public int UnitNumber { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public double AreaSize { get; set; }
        public int FloorNumber { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price{ get; set; }
        public string? Discription { get; set; }
        public string Description { get; set; }= string.Empty;
        public UnitStatus status { get; set; }
        public UnitPurpose Purpose { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
        public UnitType type { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public List<UnitSupImg> UnitSupImgs { get; set; }
        public int propertyId { get; set; }
        public Property? Property { get; set; }

        //public string userId { get; set; }
        //public User? User { get; set; }



    }
}
