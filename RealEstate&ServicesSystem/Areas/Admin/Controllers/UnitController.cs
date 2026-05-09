using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RealEstate_ServicesSystem.DATABS;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Utilities.DBinitializer;
using RealEstate_ServicesSystem.Viewmodel;
using System;
using System.Threading.Tasks;

namespace RealEstate_ServicesSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UnitController : Controller
    {
        private readonly IRepository<Unit> _unitRepository;
        private readonly IRepository<Property> _propertyRepository;
        private readonly IRepository<UnitSupImg> _unitSupImgRepository;
        private readonly ISupImgRepository _supImgRepository;
        private readonly UserManager<Applicationuser> _userManager;
        private readonly IEmailSender _emailSender;
        public UnitController(IRepository<Unit> unitRepository, IRepository<Property> propertyRepository, IRepository<UnitSupImg> unitSupImgRepository, ISupImgRepository supImgRepository, UserManager<Applicationuser> userManager, IEmailSender emailSender)
        {
            _unitRepository = unitRepository;
            _propertyRepository = propertyRepository;
            _unitSupImgRepository = unitSupImgRepository;
            _supImgRepository = supImgRepository;
            _userManager = userManager;
            _emailSender = emailSender;
        }
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Employee},{DS.Role_Owner}")]
        public async Task<IActionResult> Allunit(FilterUnitVM filterUnitVM,CancellationToken cancellationToken, int page =1)
        {
            var units =await _unitRepository.GetAllAsync(includes: [e=>e.Property,c=>c.UnitSupImgs], cancellationToken: cancellationToken, tracking:  false);
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
            units = units.Skip((page - 1) * pageSize).Take(pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)units.Count() / pageSize);


            return View(units.ToList());
        }
        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Employee}")]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.GetoneAsync(u => u.Id == id, includes: [e => e.Property, c => c.UnitSupImgs], cancellationToken: cancellationToken);

            if(unit == null)
            {
                return NotFound();
            }

            return View(unit);
        }
        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee},{DS.Role_Agent}")]
        public async Task<IActionResult> Add(CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            var vm = new AddUnitmodel()
            {
                Property =await _propertyRepository.GetAllAsync(e => e.ApplicationuserId == user.Id,cancellationToken:cancellationToken,tracking:false),
                Properties= await _propertyRepository.GetoneAsync(e=>e.ApplicationuserId == user.Id, includes: [equals=>equals.Applicationuser],cancellationToken:cancellationToken,tracking:false),

            };

            return View(vm);
        }
        [HttpPost]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee},{DS.Role_Agent}")]
        public async Task<IActionResult> Add(AddUnitmodel addUnitmodel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(addUnitmodel);
            }
            try
            { 
            var user =await _userManager.GetUserAsync(User);
            var units = await _unitRepository.GetAllAsync(includes:[p => p.Property,p => p.UnitSupImgs],cancellationToken:cancellationToken);
                if (ModelState.IsValid)
                {
                    var unit = new Unit()
                    {
                        UnitNumber = addUnitmodel.UnitNumber,
                        Bedrooms = addUnitmodel.Bedrooms,
                        Bathrooms = addUnitmodel.Bathrooms,
                        AreaSize = addUnitmodel.AreaSize,
                        FloorNumber = addUnitmodel.FloorNumber,
                        Price = addUnitmodel.Price,
                        Description = addUnitmodel.Description,
                        status = addUnitmodel.status,
                        Purpose = addUnitmodel.Purpose,
                        type = addUnitmodel.type,
                        propertyId = addUnitmodel.propertyId,
                    };
                    if (addUnitmodel.ImageUrl != null && addUnitmodel.ImageUrl.Length > 0)
                    {
                        var FileName = Guid.NewGuid().ToString() + Path.GetExtension(addUnitmodel.ImageUrl.FileName);
                        var Filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UnitImages", FileName);
                        using (var stream = new FileStream(Filepath, FileMode.Create))
                        {
                            addUnitmodel.ImageUrl.CopyTo(stream);
                        }
                        unit.ImageUrl = FileName;
                    }

                    await _unitRepository.AddAsync(unit, cancellationToken);
                    await _unitRepository.SaveChangesAsync(cancellationToken);

                    if (addUnitmodel.UnitSupImgs != null && addUnitmodel.UnitSupImgs.Count > 0)
                    {
                        foreach (var img in addUnitmodel.UnitSupImgs)
                        {
                            var FileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                            var Filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UnitSupImages", FileName);
                            using (var stream = new FileStream(Filepath, FileMode.Create))
                            {
                                img.CopyTo(stream);
                            }
                            var unitSupImg = new UnitSupImg()
                            {
                                UnitId = unit.Id,
                                ImageUrl = FileName,
                            };
                            await _unitSupImgRepository.AddAsync(unitSupImg, cancellationToken);
                        }
                        await _unitSupImgRepository.SaveChangesAsync(cancellationToken);
                    }
                    TempData["success"] = "Unit added successfully.";

                    var subject = "Unit Added Successfully ✅";

                    var message = $@"
                    <div style='font-family:Arial; text-align:center; padding:10px'>
                        <h3 style='color:#198754;'>Hello {user.Email}👋</h3>
                        <h3 style='color:#198754;'>🎉 Your Unit Has Been Added Successfully</h3>
                        <p>Your unit number is:</p>
                        <h2 style='color:#0d6efd; margin:10px 0'>{unit.UnitNumber}</h2>
                        <p style='font-size:14px; color:#555'>
                            Please keep this number for future reference..
                        </p>
                    </div>
                    ";
                    await _emailSender.SendEmailAsync(user.Email!, subject, message);

                    user.paid = false;
                    await _userManager.UpdateAsync(user);
                    if (User.IsInRole(DS.Role_Owner))
                    {
                        TempData["success"] = "Unit added successfully.";
                        return RedirectToAction("ViewOwner", "Home", new { area = "User" });
                    }

                        TempData["success"] = "Unit added successfully.(By Admin)";
                        return RedirectToAction(nameof(Allunit));
                }
            }catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while adding the unit: {ex.Message}");
            }
            return View(addUnitmodel);
        }
        [HttpGet]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee},{DS.Role_Agent}")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.GetoneAsync(e=>e.Id == id, includes: [p => p.Property], cancellationToken: cancellationToken);
            var unitSupImgs = await _unitSupImgRepository.GetAllAsync(s => s.UnitId == id, cancellationToken: cancellationToken);
            if (unit == null)
            {
                return NotFound();
            }
            var vm = new EditUnitModel()
            {
                Id = unit.Id,
                UnitNumber = unit.UnitNumber,
                Bedrooms = unit.Bedrooms,
                Bathrooms = unit.Bathrooms,
                AreaSize = unit.AreaSize,
                FloorNumber = unit.FloorNumber,
                Price = unit.Price,
                Description = unit.Description,
                status = unit.status,
                Purpose = unit.Purpose,
                type = unit.type,
                propertyId = unit.propertyId,
                Property = (List<Property>)await _propertyRepository.GetAllAsync(cancellationToken: cancellationToken),
                ExistingImageUrl = unit.ImageUrl,
                ExistingUnitSupImgs = (List<UnitSupImg>)unitSupImgs,
            };
            return View(vm);
        }
        [HttpPost]
        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee},{DS.Role_Agent}")]
        public async Task<IActionResult> Edit(EditUnitModel editUnitModel, CancellationToken cancellationToken)
        {
            var proparty = (List<Property>)await _propertyRepository.GetAllAsync(cancellationToken: cancellationToken);
            editUnitModel.Property = proparty;
            var supimgs = (List<UnitSupImg>)await _unitSupImgRepository.GetAllAsync(s => s.UnitId == editUnitModel.Id, cancellationToken: cancellationToken);
            editUnitModel.ExistingUnitSupImgs = supimgs;
     
            if (ModelState.IsValid)
            {
                var unit = await _unitRepository.GetoneAsync(u => u.Id == editUnitModel.Id, includes: [p => p.Property, p => p.UnitSupImgs], cancellationToken: cancellationToken);
                var unitSupImgs = (List<UnitSupImg>)await _unitSupImgRepository.GetAllAsync(s => s.UnitId == editUnitModel.Id, cancellationToken: cancellationToken);

                if (unit == null)
                {
                    return NotFound();
                }

                unit.UnitNumber = (int)editUnitModel.UnitNumber!;
                unit.Bedrooms = (int)editUnitModel.Bedrooms!;
                unit.Bathrooms = (int)editUnitModel.Bathrooms!;
                unit.AreaSize = (double)editUnitModel.AreaSize!;
                unit.FloorNumber = (int)editUnitModel.FloorNumber!;
                unit.Price = (decimal)editUnitModel.Price!;
                unit.Description = editUnitModel.Description!;
                unit.status = (UnitStatus)editUnitModel.status!;
                unit.Purpose = (UnitPurpose)editUnitModel.Purpose!;
                unit.type = (UnitType)editUnitModel.type!;
                unit.propertyId = (int)editUnitModel.propertyId!;

                if (editUnitModel.ImageUrl != null && editUnitModel.ImageUrl.Length > 0)
                {
                    var FileName = Guid.NewGuid().ToString() + Path.GetExtension(editUnitModel.ImageUrl.FileName);
                    var Filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UnitImages", FileName);

                    var ExistingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UnitImages", unit.ImageUrl);
                    if (System.IO.File.Exists(ExistingFilePath))
                    {
                        System.IO.File.Delete(ExistingFilePath);
                    }

                    using (var stream = new FileStream(Filepath, FileMode.Create))
                    {
                        editUnitModel.ImageUrl.CopyTo(stream);
                    }
                    unit.ImageUrl = FileName;
                }

                _unitRepository.Update(unit);
                await _unitRepository.SaveChangesAsync(cancellationToken);

                if (editUnitModel.UnitSupImgs != null && editUnitModel.UnitSupImgs.Count > 0)
                {
                    foreach (var img in editUnitModel.UnitSupImgs)
                    {
                        var FileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                        var Filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UnitSupImages", FileName);

                        if (editUnitModel.DeletedImages != null)
                        {
                            var deletedIds = string.IsNullOrEmpty(editUnitModel.DeletedImages)
                           ? new List<int>()
                           : editUnitModel.DeletedImages
                               .Split(',')
                               .Select(int.Parse)
                               .ToList();

                            var imgsToDelete = await _unitSupImgRepository.GetAllAsync(x => deletedIds.Contains(x.Id), cancellationToken:cancellationToken);
                          

                            foreach (var img1 in imgsToDelete)
                            {
                                var path = Path.Combine(
                                    Directory.GetCurrentDirectory(),
                                    "wwwroot/UnitSupImages",
                                    img1.ImageUrl);

                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                }

                                _unitSupImgRepository.Delete(img1);
                            }
                        }

                        using (var stream = new FileStream(Filepath, FileMode.Create))
                        {
                            img.CopyTo(stream);
                        }
                        var unitSupImg = new UnitSupImg()
                        {
                            UnitId = unit.Id,
                            ImageUrl = FileName,
                        };
                        await _unitSupImgRepository.AddAsync(unitSupImg, cancellationToken);
                    }
                    await _unitSupImgRepository.SaveChangesAsync(cancellationToken);

                }
                else
                {

                    if (unitSupImgs != null && unitSupImgs.Count > 0)
                    {

                        //unit.UnitSupImgs = unit.UnitSupImgs.Where(img1 => !editUnitModel.DeletedImages.Contains(img1.Id)).ToList();


                        if (editUnitModel.DeletedImages != null)
                        {
                            var deletedIds = string.IsNullOrEmpty(editUnitModel.DeletedImages)
                          ? new List<int>()
                          : editUnitModel.DeletedImages
                              .Split(',')
                              .Select(int.Parse)
                              .ToList();
                            var imgsToDelete = await _unitSupImgRepository.GetAllAsync(x => deletedIds.Contains(x.Id), cancellationToken:cancellationToken);

                            foreach (var img1 in imgsToDelete)
                            {
                                var path = Path.Combine(
                                    Directory.GetCurrentDirectory(),
                                    "wwwroot/UnitSupImages",
                                    img1.ImageUrl);

                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                }

                                _unitSupImgRepository.Delete(img1);
                            }
                        }
                        await _unitSupImgRepository.SaveChangesAsync(cancellationToken);
                    }
                }
                if (User.IsInRole(DS.Role_Owner))
                {
                    TempData["success"] = "Unit updated successfully.";
                    return RedirectToAction("ViewOwner", "Home", new { area = "User" });
                }
 
                    TempData["success"] = "Unit updated successfully.(by Admin)";

                    return RedirectToAction(nameof(Allunit));
            }else
             {
                ModelState.AddModelError(string.Empty, "Please correct the errors and try again.");
                return View(editUnitModel);
            }
        }

        [Authorize(Roles = $"{DS.Role_Admin},{DS.Role_Owner},{DS.Role_Employee},{DS.Role_Agent}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.GetoneAsync(u => u.Id == id, cancellationToken:cancellationToken);
            var unitSupImgs = (List<UnitSupImg>) await _unitSupImgRepository.GetAllAsync(s => s.UnitId == id, cancellationToken:cancellationToken);
            if (unit == null)
            {
                return NotFound();
            }
            var ExistingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UnitImages", unit.ImageUrl);
            if (System.IO.File.Exists(ExistingFilePath))
            {
                System.IO.File.Delete(ExistingFilePath);
            }
            if (unitSupImgs != null && unitSupImgs.Count > 0)
            {
                foreach (var img in unitSupImgs)
                {
                    var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UnitSupImages", img.ImageUrl);
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                   
                }
                _supImgRepository.RemoveAll(unitSupImgs);
                //_unitSupImgRepository.DeleteRange(unitSupImgs);
            }
            _unitRepository.Delete(unit);
            await _unitSupImgRepository.SaveChangesAsync(cancellationToken);
            await _unitRepository.SaveChangesAsync(cancellationToken);

            if (User.IsInRole(DS.Role_Owner))
            {
                TempData["success"] = "Unit deleted successfully.";
                return RedirectToAction("ViewOwner", "Home", new { area = "User" });
            }

                TempData["success"] = "Unit deleted successfully.(by Admin)";
                return RedirectToAction(nameof(Allunit));
        }
    }
}

  

