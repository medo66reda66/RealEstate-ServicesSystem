using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class ForgetpasswordVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }
}
