using Microsoft.AspNetCore.Identity;
using RealEstate_ServicesSystem.Models;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class AllUserVM
    {
        public List<Applicationuser> users { get; set; }
        public IdentityRole? userRole { get; set; }

    }
}
