using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate_ServicesSystem.Utilities.DBinitializer;

namespace RealEstate_ServicesSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Employee}")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
