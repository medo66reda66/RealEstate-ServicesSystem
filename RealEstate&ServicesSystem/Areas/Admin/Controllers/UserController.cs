using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Utilities.DBinitializer;
using RealEstate_ServicesSystem.Viewmodel;
using System.Threading.Tasks;

namespace RealEstate_ServicesSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
 
        private readonly IRepository<IdentityRole> _userRoleRepository;
        private readonly UserManager<Applicationuser> _userManager;


        public UserController( IRepository<IdentityRole> userRoleRepository, UserManager<Applicationuser> userManager)
        {
            _userRoleRepository = userRoleRepository;
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Employee}")]
        public async Task<IActionResult> Index(AllUserVM allUserVM,FilterUserVM filterUserVM ,CancellationToken cancellationToken,int page = 1)
        {
            var users = _userManager.Users.ToList(); 
            if (!string.IsNullOrEmpty(filterUserVM.Email))
            {
                users = users.Where(u => u.Email.Contains(filterUserVM.Email.Trim())).ToList();
            }
            if (!string.IsNullOrEmpty(filterUserVM.FullName))
            {
                users = users.Where(u => u.FullName.Contains(filterUserVM.FullName.Trim())).ToList();
            }
            if (!string.IsNullOrEmpty(filterUserVM.Role))
            {
                users = users.Where(u => _userManager.GetRolesAsync(u).Result.Contains(filterUserVM.Role)).ToList();
            }

                ViewBag.TotalPages = (int)Math.Ceiling(users.Count() / 10.0);
                users = users.Skip((page - 1) * 10).Take(10).ToList();
                ViewBag.CurrentPage = page;

                ViewBag.Roles = (await _userRoleRepository.GetAllAsync(cancellationToken: cancellationToken, tracking: false))
                .Select(r => r.Name)
                .ToList()
                ;

            return View(users);
        }
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Employee}")]
        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if(await _userManager.IsInRoleAsync(user, DS.Role_Admin))
            {
                TempData["error"] = "❌ You can't block/unblock an admin user";
                return RedirectToAction(nameof(Index));
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                // فك البلوك
                user.LockoutEnd = DateTime.UtcNow;
                TempData["success"] = $"🔓 {user.FullName} unblocked successfully";
            }
            else
            {
                // بلوك
                user.LockoutEnd = DateTime.UtcNow.AddDays(5);
                TempData["success"] = $"🔒 {user.FullName} blocked successfully";
            }
          await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }


    }
}
