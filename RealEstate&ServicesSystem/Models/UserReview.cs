namespace RealEstate_ServicesSystem.Models
{
    public class UserReview
    {
           public int Id { get; set; }
            public int? Rating { get; set; }
            public string? Comment { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public int UserRequestId { get; set; }
            public Userrequest? UserRequest { get; set; }
            public int ListingId { get; set; }
            public Listing? Listing { get; set; }
             public string UserId { get; set; }
             public Applicationuser? User { get; set; }
    }
 }

