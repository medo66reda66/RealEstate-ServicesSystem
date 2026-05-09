using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class ApplicatinUserVM
    {
        public int Id { get; set; }
         public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
    }
}
