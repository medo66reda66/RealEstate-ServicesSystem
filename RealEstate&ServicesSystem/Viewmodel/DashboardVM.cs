using RealEstate_ServicesSystem.Models;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public class DashboardVM
    {
        public int TotalProperties { get; set; }
        public int TotalUnits { get; set; }
        public int TotalListings { get; set; }
        public int TotalRequests { get; set; }

        public List<string> Months { get; set; }
        public List<int> ListingsPerMonth { get; set; }

        public List<Listing> LatestListings { get; set; }
        public List<Userrequest> RecentRequests { get; set; }
    
   }
}
