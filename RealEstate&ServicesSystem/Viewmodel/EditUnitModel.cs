using RealEstate_ServicesSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class EditUnitModel
    {
        public int Id { get; set; }
        [Required]
        public int? UnitNumber { get; set; }
        [Required]
        public int? Bedrooms { get; set; }
        [Required]
        public int? Bathrooms { get; set; }
        [Required]
        public double? AreaSize { get; set; }
        [Required]
        public int? FloorNumber { get; set; }
        public string? ExistingImageUrl { get; set; }
        public IFormFile? ImageUrl { get; set; }
        [Required]
        public decimal? Price { get; set; }
        [Required]
        public string? Description { get; set; } = string.Empty;
        [Required]
        public UnitStatus? status { get; set; }
        [Required]
        public UnitPurpose? Purpose { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public UnitType? type { get; set; }
        public List<IFormFile>? UnitSupImgs { get; set; }
        public int? propertyId { get; set; }
        public List<Property>? Property { get; set; }
        public string? DeletedImages { get; set; }
        public List<UnitSupImg>? ExistingUnitSupImgs { get; set; }
    }
}
