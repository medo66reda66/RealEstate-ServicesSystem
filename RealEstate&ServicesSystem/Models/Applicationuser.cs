using Microsoft.AspNetCore.Identity;

namespace RealEstate_ServicesSystem.Models
{
    public class Applicationuser: IdentityUser
    {
        public string FullName { get; set; }
        public string? Address { get; set; }
        public bool paid { get; set; }
    }
}
