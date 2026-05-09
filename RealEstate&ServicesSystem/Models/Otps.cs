namespace RealEstate_ServicesSystem.Models
{
    public class Otps
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public Applicationuser? Applicationuser { get; set; }
        public string Otp { get; set; }
        public bool Isvalid { get; set; }
        public DateTime ExpireAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
