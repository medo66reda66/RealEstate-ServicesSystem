using RealEstate_ServicesSystem.Models;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class ListingDetails
    {
        public Listing? listings { get; set; }
        public Unit? units { get; set; }
        public Property? properties { get; set; }
        public List<UnitSupImg> unitsSupImg { get; set; }
    }
}
