using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Utilities.DBinitializer;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace RealEstate_ServicesSystem.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class PaymentController : Controller
    {
        private readonly UserManager<Applicationuser> _userManager;
        private readonly IRepository<Listing> _listingRepository;
        private readonly IRepository<Unit> _unitRepository;
        private readonly IRepository<Property> _propertyRepository;

        public PaymentController(UserManager<Applicationuser> userManager, IRepository<Listing> listingRepository, IRepository<Unit> unitRepository, IRepository<Property> propertyRepository)
        {
            _userManager = userManager;
            _listingRepository = listingRepository;
            _unitRepository = unitRepository;
            _propertyRepository = propertyRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> pay(int? id, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.Id == null)
            {
                return NotFound();
            }

            var units = await _unitRepository.GetoneAsync(e => e.Id == id, includes: [pay => pay.Property], cancellationToken: cancellationToken);
            if (User.IsInRole(DS.Role_Agent))
            {
                try
                {
                    var returnUrl = Url.Action("Add", "Listing", new { area = "Admin", id = units.Id });
                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                        SuccessUrl = $"{Request.Scheme}://{Request.Host}/Identity/Payment/Success?returnUrl={returnUrl}",
                        CancelUrl = $"{Request.Scheme}://{Request.Host}/customer/MyTrips/cancel",
                    };

                    options.LineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "egp",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Add listing {units.Property.City} {units.Property.Location}"
                            },
                            UnitAmount = (long)200 * 100,
                        },
                        Quantity = 1,
                    });

                    var service = new SessionService();
                    var session = service.Create(options);
                    return Redirect(session.Url);
                }
                catch (Exception ex)
                {

                    return BadRequest(new { error = ex.Message });
                }
            }
            else if (User.IsInRole(DS.Role_Owner))
            {
                try
                {
                    var returnUrl = Url.Action("Add", "Unit", new { area = "Admin" });
                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                        SuccessUrl = $"{Request.Scheme}://{Request.Host}/Identity/Payment/Success?returnUrl={returnUrl}",
                        CancelUrl = $"{Request.Scheme}://{Request.Host}/customer/MyTrips/cancel",
                    };
                    options.LineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "egp",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Add Unit"
                            },
                            UnitAmount = (long)300 * 100,
                        },
                        Quantity = 1,
                    });
                    var service = new SessionService();
                    var session = service.Create(options);
                    return Redirect(session.Url);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }
            else
            {
                return RedirectToAction("index", "Home", new { area = "User" });
            }
            ;

        }

        public async Task<IActionResult> Success(string returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);

            user.paid = true;
            await _userManager.UpdateAsync(user);

            return Redirect(returnUrl);
        }
    }
}
        
        

