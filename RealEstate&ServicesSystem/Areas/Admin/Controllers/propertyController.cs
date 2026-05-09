using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RealEstate_ServicesSystem.DATABS;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Utilities.DBinitializer;
using RealEstate_ServicesSystem.Viewmodel;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstate_ServicesSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class propertyController : Controller
    {
        //private readonly ILogger<propertyController> _logger;
        private readonly IRepository<Property> _propertyRepository;
        private readonly UserManager<Applicationuser> userManager;
        public propertyController(IRepository<Property> propertyRepository, UserManager<Applicationuser> userManager)
        {
            _propertyRepository = propertyRepository;
            this.userManager = userManager;
        }
        [HttpGet]
        [Authorize(Roles =$"{DS.Role_Admin},{DS.Role_Employee},{DS.Role_Owner}")]
        public async Task<IActionResult> Allproperty(FilterpropertyVM filterVM,CancellationToken cancellationToken, int page = 1)
        {

            var properties = await _propertyRepository.GetAllAsync(cancellationToken:cancellationToken,tracking:false);
            
                if (filterVM.city is not null)
                {
                    properties = properties.Where(e => e.City.Contains(filterVM.city.Trim()));
                    ViewBag.city = filterVM.city;
                }
                if (filterVM.location is not null)
                {
                    properties = properties.Where(e => e.Location.Contains(filterVM.location.Trim()));
                    ViewBag.location = filterVM.location;
                }
                if (filterVM.status)
                {
                    properties = properties.Where(e => e.IsAvailable == true);
                    ViewBag.status = filterVM.status;
                }

            ViewBag.totalpage = Math.Ceiling(properties.Count() / 15.0);
            properties = properties.Skip((page-1)*15).Take(15);
            ViewBag.cureentpage = page;

            return View(properties.ToList());
        }
        [Authorize(Roles =$"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee}")]
       
        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Employee}")]
        public async Task<IActionResult> Details(int id,CancellationToken cancellationToken)
        {
            var property = await _propertyRepository.GetoneAsync(p => p.Id == id, cancellationToken: cancellationToken);
            if (property == null)
            {
                return NotFound();
            }
            return View(property);
        }

        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee}")]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee}")]
        public async Task<IActionResult> Add(AddProoertymodel addProoertymodel, CancellationToken cancellationToken)
        {
            var user =await userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                var property = new Models.Property
                {
                    City = addProoertymodel.City,
                    Description = addProoertymodel.Description,
                    IsAvailable = addProoertymodel.IsAvailable,
                    Location = addProoertymodel.Location,
                    PropertyType = addProoertymodel.PropertyType,
                    Address = addProoertymodel.Address,
                    CreatedAt = DateTime.UtcNow,
                    ApplicationuserId = user!.Id

                };
                await _propertyRepository.AddAsync(property, cancellationToken);
                await _propertyRepository.SaveChangesAsync(cancellationToken);

                //DBcontext.Properties.Add(property);
                //DBcontext.SaveChanges();
                if (User.IsInRole(DS.Role_Owner))
                {
                    TempData["success"] = "Property added successfully.";
                    return RedirectToAction("ViewOwner", "Home", new { area = "User" });
                }
                
                    TempData["success"] = "Property added successfully.(By Admin)";
                    return RedirectToAction("Allproperty");
            }
            else
            {
                return View(addProoertymodel);
            }
        }
        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee}")]
        public IActionResult Edit(int id)
        {
                var property = _propertyRepository.GetoneAsync(p => p.Id == id).Result;
                if (property == null)
                {
                    return NotFound();
                }
                var editModel = new EditProoertymodel
                {
                    Id = property.Id,
                    City = property.City,
                    Description = property.Description,
                    IsAvailable = property.IsAvailable,
                    Location = property.Location,
                    PropertyType = property.PropertyType,
                    Address = property.Address
                };

                return View(editModel);
        }
        [HttpPost]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee}")]
        public async Task<IActionResult> Edit(EditProoertymodel editProoertymodel,CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var property = _propertyRepository.GetoneAsync(p => p.Id == editProoertymodel.Id, cancellationToken: cancellationToken).Result;
                if (property == null)
                {
                    return NotFound();
                }
                property.City = editProoertymodel.City;
                property.Description = editProoertymodel.Description;
                property.IsAvailable = editProoertymodel.IsAvailable;
                property.Location = editProoertymodel.Location;
                property.PropertyType = editProoertymodel.PropertyType;
                property.Address = editProoertymodel.Address;

                await _propertyRepository.SaveChangesAsync(cancellationToken: cancellationToken);

                if (User.IsInRole(DS.Role_Owner))
                {
                    TempData["success"] = "Property updated successfully.";
                    return RedirectToAction("ViewOwner", "Home", new { area = "User" });
                } 

                    TempData["success"] = "Property updated successfully.(By Admin)";
                    return RedirectToAction("Allproperty");

            } else
            {
                return View(editProoertymodel);
            }
        }
    
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee}")]
        public async Task<IActionResult> Delete(int id,CancellationToken cancellationToken)
        {
            var property = await _propertyRepository.GetoneAsync(p => p.Id == id, cancellationToken: cancellationToken);
            if (property == null)
            {
                return NotFound();
            }

            _propertyRepository.Delete(property);
            await _propertyRepository.SaveChangesAsync(cancellationToken);

            if (User.IsInRole(DS.Role_Owner))
            {
                TempData["success"] = "Property deleted successfully.";
                return RedirectToAction("ViewOwner", "Home", new { area = "User" });
            }
            TempData["success"] = "Property deleted successfully.(By Admin)";
            return RedirectToAction("Allproperty");
        }
    }
}
