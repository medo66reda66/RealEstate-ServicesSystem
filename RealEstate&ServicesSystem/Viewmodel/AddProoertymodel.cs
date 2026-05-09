using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class AddProoertymodel
    {
        [Required]
        [MaxLength(100)]
        [MinLength(3)]
        public string City  { get; set; } = string.Empty;
        [Required]
            public string Description { get; set; } = string.Empty;
            public bool IsAvailable { get; set; }
        [Required]
            public string Location { get; set; } = string.Empty;
        [Required]
            public string PropertyType { get; set; } = string.Empty;
        [Required]
            public string Address { get; set; } = string.Empty;
            public DateTime Create { get; set; } = DateTime.UtcNow;

    }
}
