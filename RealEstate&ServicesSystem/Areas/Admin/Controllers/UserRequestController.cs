using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Viewmodel;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using RealEstate_ServicesSystem.SignalR.NotificationSignalR;

namespace RealEstate_ServicesSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserRequestController : Controller
    {
        private readonly IRepository<Userrequest> _userrequestRepository;
        private readonly IRepository<Listing> _listingRepository;
        private readonly IRepository<Applicationuser> _applicationuserRepository;
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<UserReview> _reviewRepository;
        private readonly UserManager<Applicationuser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IHubContext<NotificationHub> _hubContext;

        public UserRequestController(IRepository<Userrequest> userrequestRepository, IRepository<Listing> listingRepository, UserManager<Applicationuser> userManager, IEmailSender emailSender, IRepository<Applicationuser> applicationuserRepository, IRepository<Notification> notificationRepository, IRepository<UserReview> reviewRepository, IHubContext<NotificationHub> hubContext )
        {
            _userrequestRepository = userrequestRepository;
            _listingRepository = listingRepository;
            _userManager = userManager;
            _emailSender = emailSender;
            _applicationuserRepository = applicationuserRepository;
            _notificationRepository = notificationRepository;
            _reviewRepository = reviewRepository;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(ViewRequests viewRequests,FilterRequestVM filterRequestVM,CancellationToken cancellationToken,int page = 1)
        {
           
            var Requests =await _userrequestRepository.GetAllAsync(includes:[equals=>equals.Applicationuser!,equals=>equals.Listing!,equals=>equals.Listing.ApplicationUser!,equals=>equals.Listing.Unit!,equals=>equals.Listing.Unit.Property,equals=>equals.Listing.Unit.Property.Applicationuser!],cancellationToken:cancellationToken,tracking:false);

            if(filterRequestVM.ListingId != null)
            {
                Requests = Requests.Where(r => r.Listing.Unit.UnitNumber == filterRequestVM.ListingId);
            }
            if (!string.IsNullOrEmpty(filterRequestVM.EmailFrom))
            {
                Requests = Requests.Where(r => r.Applicationuser.Email.Contains(filterRequestVM.EmailFrom.Trim()));
            }
            if (!string.IsNullOrEmpty(filterRequestVM.EmailTo))
            {
                Requests = Requests.Where(r => r.Listing.ApplicationUser.Email.Contains(filterRequestVM.EmailTo.Trim()));
            }

           
            ViewBag.TotalPages = (int)Math.Ceiling(Requests.Count() / 5.0);
            Requests = Requests.Skip((page - 1) * 5).Take(5).ToList();
            ViewBag.CurrentPage = page;

            var req = new ViewRequests()
            {
                Userrequest = (List<Userrequest>)Requests,
                Userrequest1 = new Userrequest(),
            };

            return View(req);
        }
        [HttpGet]
        public async Task<IActionResult> AddRequest(int id ,CancellationToken cancellationToken)
        {
            var req = new AddRequestVM()
            {
                Listing = await _listingRepository.GetoneAsync(e=>e.Id == id , includes: [e => e.Unit!,equals=>equals.Unit.Property], cancellationToken: cancellationToken)
            };
            return View(req);
        }
        [HttpPost]
        public async Task<IActionResult> AddRequest(AddRequestVM addRequestVM ,CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(addRequestVM);
            }
            try
            {
                var sentUser = await _userManager.GetUserAsync(User);
                
                var userreq = await _applicationuserRepository.GetoneAsync(e => e.Id == addRequestVM.Listing.ApplicationUserId, cancellationToken: cancellationToken);
                var req = new Userrequest()
                {
                    msg = addRequestVM.msg,
                    RequestType = addRequestVM.RequestType,
                    ReuquestAt = addRequestVM.ReuquestAt,
                    ListingId = addRequestVM.Listing.Id,
                    ApplicationuserId = sentUser.Id

                };
                await _userrequestRepository.AddAsync(req, cancellationToken: cancellationToken);
                await _userrequestRepository.SaveChangesAsync(cancellationToken);
                
                var listing = await _listingRepository.GetoneAsync(e => e.Id == addRequestVM.Listing.Id, includes: [e => e.Unit!, equals => equals.Unit.Property], cancellationToken: cancellationToken);
                HttpContext.Session.SetString("CanReview_" +listing.Id + "_" + sentUser.Id, "true");

                var notification = new Notification()
                {
                    Message = $"You have a new request for listing #{listing.Unit.UnitNumber}.",
                    UserId = listing.ApplicationUserId,
                    FromUserId = sentUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    Url= Url.Action("Details", "Home", new { area = "User", id = listing.Id }, protocol: Request.Scheme),
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notification, cancellationToken: cancellationToken);
                await _notificationRepository.SaveChangesAsync(cancellationToken);


                await _hubContext.Clients.User(notification.UserId).SendAsync("ReceiveNotification", notification.Message, notification.Url);

        

                await _emailSender.SendEmailAsync(
                       userreq.Email,
                 $"New Request from {sentUser.UserName}",
                             $@"
                    <h3>New Request Received</h3>
                    <p>You have a new request for listing <strong>#{listing.Unit.UnitNumber}</strong>.</p>
                    <ul>
                        <li><strong>Unit City:</strong> {listing.Unit.Property.City}</li>
                        <li><strong>Unit Location:</strong> {listing.Unit.Property.Location}</li>
                        <li><strong>Status:</strong> {addRequestVM.RequestType}</li>
                        <li><strong>Message:</strong> {addRequestVM.msg}</li>
                        <li><strong>Message:</strong> {userreq.PhoneNumber}</li>
                    </ul>
                    <p>Best regards,<br/>Real Estate System</p>
                    ");

                await _emailSender.SendEmailAsync(
                        sentUser.Email,
                        $"Your Request for {listing.Unit.UnitNumber} has been sent",
                                 $@"
                        <h3>Request Sent Successfully</h3>
                        <p>Your request for listing <strong>#{listing.Unit.UnitNumber}</strong> has been sent to the owner.</p>
                        <ul>
                            <li><strong>Unit City:</strong> {listing.Unit.Property.City}</li>
                            <li><strong>Unit Location:</strong> {listing.Unit.Property.Location}</li>
                            <li><strong>Status:</strong> {addRequestVM.RequestType}</li>
                            <li><strong>Message:</strong> {addRequestVM.msg}</li>
                        </ul>
                        <p>You will be contacted by the owner soon.<br/>Best regards,<br/>Real Estate System</p>
                        ");

                TempData["success"] = "Your request has been sent successfully!";

            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred while sending your request: {ex.Message}";
                return View(addRequestVM);
            }

            return RedirectToAction("Index","Home",new { area = "User" });
        }
    }
}
