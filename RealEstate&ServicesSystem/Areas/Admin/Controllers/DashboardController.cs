using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Utilities.DBinitializer;
using RealEstate_ServicesSystem.Viewmodel;
using System.Linq.Expressions;

namespace RealEstate_ServicesSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IRepository<Property> _propertyRepo;
        private readonly IRepository<Unit> _unitRepo;
        private readonly IRepository<Listing> _listingRepo;
        private readonly IRepository<Userrequest> _requestRepo;

        public DashboardController(
            IRepository<Property> propertyRepo,
            IRepository<Unit> unitRepo,
            IRepository<Listing> listingRepo,
            IRepository<Userrequest> requestRepo)
        {
            _propertyRepo = propertyRepo;
            _unitRepo = unitRepo;
            _listingRepo = listingRepo;
            _requestRepo = requestRepo;
        }

        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Employee}")]
        public async Task<IActionResult> Index()
        {
            var properties = await _propertyRepo.GetAllAsync();
            var units = await _unitRepo.GetAllAsync();
            var listings = await _listingRepo.GetAllAsync(includes: new Expression<Func<Listing, object>>[] { l => l.Unit! });
            var requests = await _requestRepo.GetAllAsync();

            var model = new DashboardVM
            {
                TotalProperties = properties.Count(),
                TotalUnits = units.Count(),
                TotalListings = listings.Count(),
                TotalRequests = requests.Count(),
                
                LatestListings = listings.OrderByDescending(l => l.createAt).Take(5).ToList(),
                RecentRequests = requests.OrderByDescending(r => r.ReuquestAt).Take(5).ToList(),
                
                Months = new List<string>(),
                ListingsPerMonth = new List<int>()
            };

            // Populate activity chart data for the last 6 months
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = DateTime.Now.AddMonths(-i);
                var monthName = monthDate.ToString("MMM");
                model.Months.Add(monthName);
                
                var count = listings.Count(l => l.createAt.Month == monthDate.Month && l.createAt.Year == monthDate.Year);
                model.ListingsPerMonth.Add(count);
            }

            return View(model);
        }
    }
}

