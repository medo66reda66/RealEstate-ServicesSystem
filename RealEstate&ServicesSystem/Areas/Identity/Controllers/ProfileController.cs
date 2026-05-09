using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Viewmodel;
using System.Threading.Tasks;

namespace RealEstate_ServicesSystem.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class ProfileController : Controller
    {
        private readonly UserManager<Applicationuser> _userManager;
        public ProfileController(UserManager<Applicationuser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user =await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userVM = new ApplicatinUserVM
            {
                FullName = user.FullName,
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };
             
                    return View(userVM);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Applicationuser applicationuser)
        {
            var user =await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (applicationuser.FullName is null||applicationuser.Email is null || applicationuser.Address is null && applicationuser.PhoneNumber is null)
            {
                TempData["error"] = "Must Have Value";
                return NotFound();
            }

            user.FullName = applicationuser.FullName;
            user.Email = applicationuser.Email;
            user.Address = applicationuser.Address;
            user.PhoneNumber = applicationuser.PhoneNumber;

           await _userManager.UpdateAsync(user);
            TempData["success"] = "profile Update successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> UpdatePassword(ApplicatinUserVM applicatinUserVM)
        {
            var user =await _userManager.GetUserAsync (User);

            if(!ModelState.IsValid)
            {
              
                return View(applicatinUserVM);
            }
            if(applicatinUserVM.CurrentPassword is null || applicatinUserVM.NewPassword is null)
            {
                TempData["error"] = "Must Have Value";
                return RedirectToAction (nameof(Index));
            }

            var result = await _userManager.ChangePasswordAsync(user!, applicatinUserVM.CurrentPassword, applicatinUserVM.NewPassword);
            if (!result.Succeeded)
            {
                TempData["error"] = string.Join(", ", result.Errors.Select(e=>e.Code));
                return RedirectToAction(nameof(Index));
            }

            TempData["success"] = "Password Update successfully.";

            return RedirectToAction(nameof(Index));
        }
        
    }
}
