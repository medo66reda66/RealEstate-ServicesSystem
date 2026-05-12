using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate_ServicesSystem.DATABS;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Utilities.DBinitializer;
using RealEstate_ServicesSystem.Viewmodel;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using RealEstate_ServicesSystem.SignalR.NotificationSignalR;

namespace RealEstate_ServicesSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ListingController : Controller
    {
        private readonly IRepository<Listing> _listingRepository;
        private readonly IRepository<Unit> _unitRepository;
        private readonly UserManager<Applicationuser> _userManager;
        private readonly IRepository<Notification> _notificationManager;
        private readonly IEmailSender _emailSender;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ApplicationDBcontext _context;

        public ListingController(IRepository<Listing> listingRepository, IRepository<Unit> unitRepository, UserManager<Applicationuser> userManager, IRepository<Notification> notificationManager, IEmailSender emailSender, IHubContext<NotificationHub> hubContext, ApplicationDBcontext context )
        {
            _listingRepository = listingRepository;
            _unitRepository = unitRepository;
            _userManager = userManager;
            _notificationManager = notificationManager;
            _emailSender = emailSender;
            _hubContext = hubContext;
            _context = context;
        }
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Employee},{DS.Role_Owner},{DS.Role_Agent}")]
        public async Task<IActionResult> AllListing(FilterListingVM filterListingVM,CancellationToken cancellationToken, int page = 1)
        {

            var listingas = await _listingRepository.GetAllAsync(includes:[ l => l.Unit,p=>p.Unit.Property], cancellationToken: cancellationToken,tracking: false);
            if (filterListingVM.UnitNumber is not null)
            {
                listingas = listingas.Where(l => l.Unit.UnitNumber == filterListingVM.UnitNumber).ToList();
                ViewBag.UnitNumber = filterListingVM.UnitNumber;
            }
            if (filterListingVM.City is not null)
            {
                listingas = listingas.Where(l => l.Unit.Property.City.Contains(filterListingVM.City)).ToList();
                ViewBag.City = filterListingVM.City;
            }
            if (filterListingVM.IsActive)
            {
                listingas = listingas.Where(l => l.IsActive == true).ToList();
                ViewBag.IsActive = filterListingVM.IsActive;
            }

            ViewBag.TotalPages = Math.Ceiling(listingas.Count() / 15.0);
                listingas = listingas.Skip((page - 1) * 15).Take(15).ToList();
                ViewBag.CurrentPage = page;

            if (listingas == null)
            {
                return NotFound();
            }
            return View(listingas);
        }
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Agent},{DS.Role_Employee}")]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var listing = await _listingRepository.GetoneAsync(e=>e.Unit.Id == id, includes: [l => l.Unit, p => p.Unit.Property], cancellationToken: cancellationToken, tracking: false);
            if (listing == null)
            {
                return NotFound();
            }
            return View(listing);
        }
        
        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Agent},{DS.Role_Employee}")]
        public async Task<IActionResult> Add(int id,CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var ListingFromDb = await _listingRepository.GetAllAsync(
                e => e.ApplicationUser.Id == user.Id &&
                     e.createAt >= startOfMonth,
                cancellationToken: cancellationToken,
                tracking: false
            );
            if (ListingFromDb.Count() >= 15)
            {
                TempData["error"] = "You have reached the maximum limit of 15 listings per month.Please wait until next month or delete an existing listing.";
                return RedirectToAction("MyListings", "Home", new { area = "User" });
            }
            var VM = new AddListingmodel
            {
                Units = await _unitRepository.GetoneAsync(e => e.Id == id, includes: [e => e.Property], cancellationToken: cancellationToken, tracking: false),
                Applicationuser = user
            };
            return View(VM);
        }
        [HttpPost]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Agent},{DS.Role_Employee}")]
        public async Task<IActionResult> Add(AddListingmodel addListingmodel,CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);

           
            using var transaction = await _context.Database.BeginTransactionAsync();
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please correct the errors in the form.";
                return View(addListingmodel);
            }
          
            try
            {
                var listing = new Listing()
                {
                    UnitId = addListingmodel.UnitId,
                    BrokerPercentage = addListingmodel.BrokerPercentage,
                    IsActive = (bool)addListingmodel.IsActive,
                    Rentperiod = addListingmodel.Rentperiod,
                    Description = addListingmodel.Description!,
                    createAt = addListingmodel.createAt,
                    ApplicationUserId = user.Id
                };

                await _listingRepository.AddAsync(listing, cancellationToken);
                await _listingRepository.SaveChangesAsync(cancellationToken);

                var listingFromDb = await _listingRepository.GetoneAsync(e => e.Id == listing.Id, includes: [l => l.Unit, l => l.Unit.Property.Applicationuser], cancellationToken: cancellationToken, tracking: false);
                var Owner = await _userManager.FindByIdAsync(listingFromDb.Unit.Property.ApplicationuserId);

                var Notification = new Notification
                {
                    UserId = Owner.Id,
                    Message = $"A new listing for your property {listingFromDb.Unit.UnitNumber} has been added successfully.",
                    FromUserId = user.Id,
                    Url = Url.Action("Details", "Home", new { id = listing.Id, area = "User" },protocol: Request.Scheme),
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationManager.AddAsync(Notification,cancellationToken);
                await _notificationManager.SaveChangesAsync(cancellationToken);

                // Send real-time notification
                await _hubContext.Clients.User(Owner.Id).SendAsync("ReceiveNotification", Notification.Message, Notification.Url);

                await _emailSender.SendEmailAsync(
                  user.Email,
                      "Listing Added Successfully 🎉",
                      $@"
                <div style='font-family:Arial; padding:15px; background:#f4f6f9;'>
                    <div style='max-width:500px; margin:auto; background:#fff; padding:20px; border-radius:10px; text-align:center;'>
                        <h3 style='color:#0d6efd;'>🏠 Listing Created</h3>
                        <p>Hello {user.Email},</p>
                        <p>Your listing for unit <b>{listingFromDb.Unit.UnitNumber}</b> has been added successfully.</p>
                        <p style='color:green; font-weight:bold;'>Now it is live 🎯</p>
                    </div>
                </div>
                ");

               await _emailSender.SendEmailAsync(
                  Owner.Email,
                      "New Listing Added to Your Property 🏢",
                      $@"
                <div style='font-family:Arial; padding:15px; background:#f4f6f9;'>
                    <div style='max-width:500px; margin:auto; background:#fff; padding:20px; border-radius:10px; text-align:center;'>
                        <h3 style='color:#0d6efd;'>🏢 New Listing Added</h3>
                        <p>Hello {Owner.Email},</p>
                        <p>A new listing for your property <b>{listingFromDb.Unit.UnitNumber}</b> has been added successfully.</p>
                        <p style='color:green; font-weight:bold;'>Check it out now 🎯</p>
                    </div>
                </div>
                ");

                user.paid = false;
                await _userManager.UpdateAsync(user);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                 TempData["error"] = $"An error occurred while adding the listing: {ex.Message}";
                await transaction.RollbackAsync(cancellationToken);
                return RedirectToAction(nameof(AllListing));
            }

            if (User.IsInRole(DS.Role_Admin) || User.IsInRole(DS.Role_Employee))
            {
                TempData["success"] = "Listing Add successfully!(By Admin)";
                return RedirectToAction(nameof(AllListing));
            }
            
                TempData["success"] = "Listing Add successfully!";
                return RedirectToAction("MyListings", "Home", new { area = "User" });
        }

        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Agent},{DS.Role_Employee},{DS.Role_Owner}")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            var listing = await _listingRepository.GetoneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            var units = await _unitRepository.GetAllAsync(includes: [u => u.Property], cancellationToken: cancellationToken, tracking: false);
            if (listing == null)
            {
                return NotFound();
            }
            var editListingModel = new EditListingmodel
            {
                UnitId = listing.UnitId,
                BrokerPercentage = listing.BrokerPercentage,
                IsActive = listing.IsActive,
                Rentperiod = listing.Rentperiod,
                Description = listing.Description,
                createAt = listing.createAt,
                Units = units.ToList()
            };

            user.paid = false;
            await _userManager.UpdateAsync(user);

            return View(editListingModel);
        }
        [HttpPost]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Agent},{DS.Role_Employee}")]
        public async Task<IActionResult> Edit(EditListingmodel editListingModel, CancellationToken cancellationToken)
        {
            var listing = await _listingRepository.GetoneAsync(e => e.Id == editListingModel.Id, cancellationToken: cancellationToken);
            if (listing == null)
            {
                return NotFound();
            }
            listing.UnitId = (int)editListingModel.UnitId;
            listing.BrokerPercentage = editListingModel.BrokerPercentage;
            listing.IsActive = (bool)editListingModel.IsActive;
            listing.Rentperiod = editListingModel.Rentperiod;
            listing.Description = editListingModel.Description!;
            listing.createAt = editListingModel.createAt;

            await _listingRepository.SaveChangesAsync(cancellationToken);

            if (User.IsInRole(DS.Role_Admin) || User.IsInRole(DS.Role_Employee))
            {
                TempData["success"] = "Listing updated successfully!(By Admin)";
                return RedirectToAction(nameof(AllListing));
            }
       
                TempData["success"] = "Listing updated successfully!";
            return RedirectToAction("MyListings", "Home", new { area = "User" });
        }
        
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Employee},{DS.Role_Agent}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {    
            var listing = await _listingRepository.GetoneAsync(e => e.Id == id, cancellationToken: cancellationToken);
                if (listing == null)
                {
                    return NotFound();
                }

                _listingRepository.Delete(listing);
                await _listingRepository.SaveChangesAsync(cancellationToken);

            if (User.IsInRole(DS.Role_Admin) || User.IsInRole(DS.Role_Employee))
            {
                TempData["success"] = "Listing Delete successfully!(By Admin)";
                return RedirectToAction(nameof(AllListing));
            }

                TempData["success"] = "Listing Delete successfully!";
                return RedirectToAction("MyListings", "Home", new { area = "User" });
            
        }
    }
}
