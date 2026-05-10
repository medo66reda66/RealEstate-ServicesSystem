using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RealEstate_ServicesSystem.DATABS;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Utilities.DBinitializer;
using RealEstate_ServicesSystem.Viewmodel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate_ServicesSystem.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository<Listing> listingRepository;
        private readonly IRepository<Unit> unitRepository;
        private readonly IRepository<Property> propertyRepository;
        private readonly IRepository<Notification> notificationRepository;
        private readonly IRepository<Favorite> favoriteRepository;
        private readonly UserManager<Applicationuser> userManager;


        public HomeController(IRepository<Listing> listingRepository, IRepository<Unit> unitRepository, ILogger<HomeController> logger, UserManager<Applicationuser> userManager, IRepository<Property> propertyRepository, IRepository<Notification> notificationRepository, IRepository<Favorite> favoriteRepository)
        {
            this.listingRepository = listingRepository;
            this.unitRepository = unitRepository;
            _logger = logger;
            this.userManager = userManager;
            this.propertyRepository = propertyRepository;
            this.notificationRepository = notificationRepository;
            this.favoriteRepository = favoriteRepository;
        }
        public async Task<IActionResult> Favorites()
        {
            var user = await userManager.GetUserAsync(User);
            var favorites = await favoriteRepository.GetAllAsync(f => f.ApplicationUserId == user.Id, includes: [f => f.Listing, f => f.Listing.Unit, f => f.Listing.Unit.Property], cancellationToken: CancellationToken.None, tracking: false);
            return View(favorites);
        }
        [HttpPost]
        public async Task<IActionResult> RemoveFavorites(int id,CancellationToken cancellationToken)
        {
            var fav = await favoriteRepository.GetoneAsync(f => f.Id == id, cancellationToken: cancellationToken);

            if (fav != null)
            {
                favoriteRepository.Delete(fav);
                await favoriteRepository.SaveChangesAsync(cancellationToken);
            }

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int id,CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);

            var fav = await favoriteRepository.GetAllAsync(x => x.ListingId == id && x.ApplicationUserId == user.Id,cancellationToken:cancellationToken);
            var favorite = fav.FirstOrDefault();

            if (favorite == null)
            {
                await favoriteRepository.AddAsync(new Favorite
                {
                    ListingId = id,
                    ApplicationUserId = user.Id
                }, cancellationToken);
                await favoriteRepository.SaveChangesAsync(cancellationToken);


                return Json(new { isFavorite = true });
            }
            else
            {
                 favoriteRepository.Delete(favorite);
                await favoriteRepository.SaveChangesAsync(cancellationToken);

                return Json(new { isFavorite = false });
            }
        }
        public async Task<IActionResult> GetNotifications(NotificatinVM notificatinVM,CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);

            var Not = new NotificatinVM()
            {
                NotificationListing = (List<Notification>)(await notificationRepository.GetAllAsync(n => n.UserId == user.Id, includes: [n => n.FromUser], cancellationToken: cancellationToken, tracking: false))
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToList(), 
                
                NotificationOwner = (List<Notification>)(await notificationRepository.GetAllAsync(n => n.UserId == user.Id, includes: [n => n.FromUser], cancellationToken: cancellationToken, tracking: false))
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToList(),

                 Applicationuser = user
            };
            ViewBag.NotifCount = Not.NotificationListing.Count(n => !n.IsRead);
            ViewBag.NotifCountOwner = Not.NotificationOwner.Count(n => !n.IsRead);

            return View(Not);
        }
        public async Task<IActionResult> Index(FilterHome filterHome, CancellationToken cancellationToken, int page = 1)
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-9);
            var listing = ((List<Listing>)await listingRepository.GetAllAsync(e => e.Unit.status == UnitStatus.Available && e.IsActive && e.createAt >= sixMonthsAgo, includes: [l => l.Unit, l => l.Unit.Property], cancellationToken: cancellationToken, tracking: false))
                .OrderByDescending(e => e.createAt).ToList();

            if (filterHome.city != null)
            {
                listing = listing.Where(x => x.Unit.Property.City.Contains(filterHome.city.Trim())).ToList();
                ViewBag.city = filterHome.city;
            }
            if (filterHome.Location != null)
            {
                listing = listing.Where(x => x.Unit.Property.Location.Contains(filterHome.Location.Trim())).ToList();
                ViewBag.Location = filterHome.Location;
            }
            if (filterHome.MinPrice != null)
            {
                listing = listing.Where(x => x.Price >= filterHome.MinPrice).ToList();
                ViewBag.MinPrice = filterHome.MinPrice;
            }
            if (filterHome.MaxPrice != null)
            {
                listing = listing.Where(x => x.Price <= filterHome.MaxPrice).ToList();
                ViewBag.MaxPrice = filterHome.MaxPrice;
            }
            if (filterHome.MinArea != null)
            {
                listing = listing.Where(x => x.Unit.AreaSize >= filterHome.MinArea).ToList();
                ViewBag.MinArea = filterHome.MinArea;
            }
            if (filterHome.MaxArea != null)
            {
                listing = listing.Where(x => x.Unit.AreaSize <= filterHome.MaxArea).ToList();
                ViewBag.MaxArea = filterHome.MaxArea;
            }
            if (filterHome.Bedrooms != null)
            {
                listing = listing.Where(x => x.Unit.Bedrooms == filterHome.Bedrooms).ToList();
                ViewBag.Bedrooms = filterHome.Bedrooms;
            }
            if (filterHome.Bathrooms != null)
            {
                listing = listing.Where(x => x.Unit.Bathrooms == filterHome.Bathrooms).ToList();
                ViewBag.Bathrooms = filterHome.Bathrooms;
            }
            if (filterHome.UnitPurpose != null) {
                listing = listing.Where(x => x.Unit.Purpose == filterHome.UnitPurpose).ToList();
                ViewBag.UnitPurpose = filterHome.UnitPurpose;
            }
            ViewBag.HasFilter =
           !string.IsNullOrEmpty(filterHome.city) ||
           !string.IsNullOrEmpty(filterHome.Location) ||
           filterHome.MinPrice != null ||
           filterHome.MaxPrice != null ||
           filterHome.MinArea != null ||
           filterHome.MaxArea != null ||
           filterHome.Bedrooms != null ||
           filterHome.Bathrooms != null ||
           filterHome.UnitPurpose != null;

            ViewBag.totalpages = Math.Ceiling(listing.Count / 15.0);
            listing = listing.Skip((page - 1) * 15).Take(15).ToList();
            ViewBag.currentpage = page;

            return View(listing);
        }
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_User},{DS.Role_Employee},{DS.Role_Agent},{DS.Role_Owner}")]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {

            var lis = await listingRepository.GetoneAsync(e => e.Id == id, includes: [l => l.Unit, l => l.Unit.Property, l => l.Unit.UnitSupImgs,c=>c.ApplicationUser], cancellationToken: cancellationToken, tracking: false);

            return View(lis);
        }

        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee}")]
        public async Task<IActionResult> ViewOwner(ViewOwnerVM viewOwnerVM,CancellationToken cancellationToken)
        {
            var user =await userManager.GetUserAsync(User);
            var unitsproperty = new ViewOwnerVM
            {
                Property = (List<Property>)await propertyRepository.GetAllAsync(e=>e.ApplicationuserId == user!.Id,
                    cancellationToken: cancellationToken,tracking: false),
                Unit = (List<Unit>)await unitRepository.GetAllAsync(e=>e.Property.ApplicationuserId == user!.Id, includes:[equals=>equals.UnitSupImgs],
                    cancellationToken: cancellationToken,tracking: false)
            };
            return View(unitsproperty);
        }

        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee},{DS.Role_Agent}")]
        public async Task<IActionResult> CreateListing(FilterUnitVM filterUnitVM,ListingUnitVM listingUnitVM, CancellationToken cancellationToken,int page =1)
        {
            var user = await userManager.GetUserAsync(User);
            var units = await unitRepository.GetAllAsync(includes: [e => e.Property, c => c.UnitSupImgs, d => d.Property.Applicationuser], cancellationToken: cancellationToken, tracking: false);
            if (filterUnitVM.unitNumber is not null)
            {
                units = units.Where(e => e.UnitNumber.ToString().Contains(filterUnitVM.unitNumber.ToString().Trim()));
                ViewBag.UnitNumber = filterUnitVM.unitNumber;
            }
            if (filterUnitVM.bedrooms is not null)
            {
                units = units.Where(e => e.Bedrooms == filterUnitVM.bedrooms);
                ViewBag.Bedrooms = filterUnitVM.bedrooms;
            }
            if (filterUnitVM.bathrooms is not null)
            {
                units = units.Where(e => e.Bathrooms == filterUnitVM.bathrooms);
                ViewBag.Bathrooms = filterUnitVM.bathrooms;
            }
            if (filterUnitVM.areaSize is not null)
            {
                units = units.Where(e => e.AreaSize == filterUnitVM.areaSize);
                ViewBag.AreaSize = filterUnitVM.areaSize;
            }
            if (filterUnitVM.floorNumber is not null)
            {
                units = units.Where(e => e.FloorNumber == filterUnitVM.floorNumber);
                ViewBag.FloorNumber = filterUnitVM.floorNumber;
            }
            if (filterUnitVM.price is not null)
            {
                units = units.Where(e => e.Price == filterUnitVM.price);
                ViewBag.Price = filterUnitVM.price;
            }
            if (filterUnitVM.status is not null)
            {
                units = units.Where(e => e.status == filterUnitVM.status);
                ViewBag.Status = filterUnitVM.status;
            }
            if (filterUnitVM.purpose is not null)
            {
                units = units.Where(e => e.Purpose == filterUnitVM.purpose);
                ViewBag.Purpose = filterUnitVM.purpose;
            }
            if (filterUnitVM.type is not null)
            {
                units = units.Where(e => e.type == filterUnitVM.type);
                ViewBag.Type = filterUnitVM.type;
            }

            int pageSize = 15;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)units.Count() / pageSize);
            units = units.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var unitsListing = new ListingUnitVM
            {
                unit = (List<Unit>)units,
                listings = (List<Listing>)await listingRepository.GetAllAsync(e => e.ApplicationUserId == user.Id, includes: [d=>d.Unit,c=>c.Unit.Property] ,cancellationToken: cancellationToken)         
            };
            return View(unitsListing);
        }

        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee},{DS.Role_Agent}")]
        public async Task<IActionResult> MyListings(CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            var myListings = await listingRepository.GetAllAsync(
                e => e.ApplicationUserId == user!.Id, 
                includes: [d => d.Unit, c => c.Unit.Property, s => s.Unit.UnitSupImgs], 
                cancellationToken: cancellationToken, tracking: false);
            return View(myListings);
        }



    }
}
