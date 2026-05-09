using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class ResetPasswordVM
    {
        public string? Id { get; set; }

        [Required]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")] 
        public string? ConfirmNewPassword { get; set; }

        public string UserId { get; set; }
    }
}
