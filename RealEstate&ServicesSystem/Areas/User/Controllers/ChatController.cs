using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using System.Threading.Tasks;

namespace RealEstate_ServicesSystem.Areas.User.Controllers
{
    [Area("User")]
    public class ChatController : Controller
    {
        private readonly IRepository<Massage> _repo;
        private readonly UserManager<Applicationuser> _userManager;

        public ChatController(IRepository<Massage> repo,
                              UserManager<Applicationuser> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string userId)
        {
            var currentUser =await  _userManager.GetUserAsync(User);

            ViewBag.ReceiverId = userId;
            ViewBag.CurrentUserId = currentUser.Id;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser.Id;

            var messages = await _repo.GetAllAsync();

            var result = messages
                .Where(x =>
                    (x.SenderId == currentUserId && x.ReceiverId == userId) ||
                    (x.SenderId == userId && x.ReceiverId == currentUserId))
                .OrderBy(x => x.SentAt)
                .ToList();

            ViewBag.CurrentUserId = currentUser.Id;

            return Json(result);
        }
    }
}
