using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using RealEstate_ServicesSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class AddListingmodel
    {
 
        public int UnitId { get; set; }
        [ValidateNever]
        public Unit? Units { get; set; }
        public DateTime? ListedAt { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal? BrokerPercentage { get; set; }
        public bool? IsActive { get; set; }
       
        public RentPeriod? Rentperiod { get; set; }
     
        public string? Description { get; set; } = string.Empty;
        
        public Applicationuser? Applicationuser { get; set; }
        public DateTime createAt { get; set; } = DateTime.Now;
    }
}
