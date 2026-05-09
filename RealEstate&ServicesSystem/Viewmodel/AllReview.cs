using RealEstate_ServicesSystem.Models;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class AllReview
    {
        public int listingId { get; set; }
        public List<UserReview> reviews { get; set; }
    }
}
