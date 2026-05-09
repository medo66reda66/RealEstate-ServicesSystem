using Microsoft.EntityFrameworkCore;

namespace RealEstate_ServicesSystem.Models
{
    public class Notification
    {
            public int Id { get; set; }
            public string UserId { get; set; } 
            public Applicationuser? User { get; set; }
           public string? FromUserId { get; set; } 
           public Applicationuser? FromUser { get; set; }
        public string Message { get; set; }
            public bool IsRead { get; set; } = false;
            public string? Url { get; set; } 
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    }
}
