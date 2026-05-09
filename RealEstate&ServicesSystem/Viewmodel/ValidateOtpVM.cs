using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class ValidateOtpVM
    {
        public int Id { get; set; }
        [Required]
        public string Otp { get; set; }
        public string UserId { get; set; }
    }
}
