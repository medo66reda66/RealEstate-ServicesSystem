using RealEstate_ServicesSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class AddUnitmodel
    {
        [Required]
        public int UnitNumber { get; set; }
        [Required]
        public int Bedrooms { get; set; }
        [Required]
        public int Bathrooms { get; set; }
        [Required]
        public double AreaSize { get; set; }
        [Required]
        public int FloorNumber { get; set; }
        [Required]
        public IFormFile ImageUrl { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public UnitStatus status { get; set; }
        [Required]
        public UnitPurpose Purpose { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public UnitType type { get; set; }
        [Required]
        public List<IFormFile>? UnitSupImgs { get; set; }
        [Required]
        public int propertyId { get; set; }
        public IEnumerable<Property>? Property { get; set; }
        public Property? Properties { get; set; }
    }
}
