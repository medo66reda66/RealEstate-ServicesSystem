using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using RealEstate_ServicesSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class AddRequestVM
    {
        public int Id { get; set; }
        [Required]
        public RequestType? RequestType { get; set; }
        [Required]
        public string msg { get; set; } = string.Empty;
        public int ListingId { get; set; }
        [ValidateNever]
        public Listing? Listing { get; set; }
        public DateTime ReuquestAt { get; set; } = DateTime.UtcNow;
        //public string ApplicationuserId { get; set; }
    }
}
