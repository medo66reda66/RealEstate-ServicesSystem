using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Utilities.DBinitializer;
using RealEstate_ServicesSystem.Viewmodel;
using System.Threading.Tasks;

namespace RealEstate_ServicesSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReviewController : Controller
    {

        private readonly IRepository<Userrequest> _userrequestRepository;
        private readonly IRepository<Listing> _listingRepository;
        private readonly IRepository<Applicationuser> _applicationuserRepository;
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<UserReview> _reviewRepository;
        private readonly UserManager<Applicationuser> _userManager;
      

        public ReviewController(IRepository<Userrequest> userrequestRepository, IRepository<Listing> listingRepository, UserManager<Applicationuser> userManager, IRepository<Applicationuser> applicationuserRepository, IRepository<Notification> notificationRepository, IRepository<UserReview> reviewRepository)
        {
            _userrequestRepository = userrequestRepository;
            _listingRepository = listingRepository;
            _userManager = userManager;
            _applicationuserRepository = applicationuserRepository;
            _notificationRepository = notificationRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<IActionResult> AllNotificationUserListing(int id,CancellationToken cancellationToken)
        {
            var reviews =await _reviewRepository.GetAllAsync(r => r.ListingId == id, includes:[equals=>equals.User,testc=>testc.Listing],cancellationToken:cancellationToken, tracking: false);
            if (reviews == null)
            {
                return NotFound();
            }
            var allReviews = new AllReview()
            {
                listingId = id,
                reviews = reviews.ToList()
            };
            return View(allReviews);
        }
        [HttpGet]
        [Authorize(Roles = $"{DS.Role_User},{DS.Role_Admin}")]
        public async Task<IActionResult> AddReview(int id, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);

            var userrequest = (await _userrequestRepository.GetoneAsync(u => u.ListingId == id , cancellationToken: cancellationToken, tracking: false));
            var canReview = HttpContext.Session.GetString("CanReview_"+id + "_" + user.Id);
            if (userrequest == null || string.IsNullOrEmpty(canReview))
            {
                TempData["error"] = "You cannot add a review for this listing as you have not made a request for it.";
                return RedirectToAction("Details","Home", new {area="User", id = id });
            }
            var Rev = new AddReviewVM
            {
                ListingId = id,
                UserRequestId = userrequest.Id
            };

            return View(Rev);
        }
        [HttpPost]
        [Authorize(Roles = $"{DS.Role_User},{DS.Role_Admin}")]
        public async Task<IActionResult> AddReview(AddReviewVM reviewVM, CancellationToken cancellationToken)
        {
            if(!ModelState.IsValid)
            {
                return View(reviewVM);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var review = new UserReview
            {
              ListingId = (int)reviewVM.ListingId,
              UserRequestId = (int)reviewVM.UserRequestId,
              UserId = user.Id,
              Comment = reviewVM.Comment,
              Rating = reviewVM.Rating,
            };
            await _reviewRepository.AddAsync(review, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);
            TempData["success"] = "Review added successfully.";

            return RedirectToAction(nameof(AllNotificationUserListing), new { id = review.ListingId });
        }
    }
}
