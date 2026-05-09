using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public enum UserRole
    {
        Agent,
        Owner,
        User
    }
    public class RegisterVM
    {
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }
        
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Role is required.")]
        public UserRole Role { get; set; }


    }
}
