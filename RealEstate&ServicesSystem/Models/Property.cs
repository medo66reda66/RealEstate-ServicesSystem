namespace RealEstate_ServicesSystem.Models
{
    public class Property
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Location { get; set; }
        public string PropertyType { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ApplicationuserId { get; set; }
        public Applicationuser? Applicationuser { get; set; }

    }
}
