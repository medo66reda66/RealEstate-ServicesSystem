using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;
using RealEstate_ServicesSystem.Migrations;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Utilities;
using RealEstate_ServicesSystem.Utilities.DBinitializer;
using RealEstate_ServicesSystem.Viewmodel;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealEstate_ServicesSystem.Areas.Identity.Controllers
{
    [Area("Identity")]
    
    public class AccountController : Controller
    {
        private readonly UserManager<Applicationuser> _userManager;
        private readonly SignInManager<Applicationuser> _signInManager;
        private readonly IEmailSender _emailsender;
         private readonly IRepository<Otps> _otpRepository;
        public AccountController(UserManager<Applicationuser> userManager, SignInManager<Applicationuser> signInManager, IEmailSender emailService, IRepository<Otps> otpRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailsender = emailService;
            _otpRepository = otpRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }
            var user = new Applicationuser
            {
                UserName = registerVM.Email,
                Email = registerVM.Email,
                FullName = registerVM.FullName,
                PhoneNumber = registerVM.PhoneNumber,
                Address = registerVM.Address,
            };
            var result = await _userManager.CreateAsync(user, registerVM.Password);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var Link = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

                await _emailsender.SendEmailAsync(registerVM.Email, "Confirm your email", $"Please confirm your email by clicking on the following link: <a href='{Link}'>Confirm Email</a>");

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Code);
                }
                return View(registerVM);
            }
            if(registerVM.Role == UserRole.Owner)
            {
                await _userManager.AddToRoleAsync(user, DS.Role_Owner);
            }
            else if(registerVM.Role == UserRole.Agent)
            {
                await _userManager.AddToRoleAsync(user, DS.Role_Agent);
            }
            else if(registerVM.Role == UserRole.User)
            {
                await _userManager.AddToRoleAsync(user, DS.Role_User);
            }
            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Id");
                return RedirectToAction(nameof(Login));
            }
           var result =  await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Email Confirmation Failed");
            }

            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public IActionResult ResentConfirmEmail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResentConfirmEmail(ResentConfirmEmailVM resentConfirmEmailVM)
        {
            var user =await _userManager.FindByEmailAsync(resentConfirmEmailVM.Email);
            if(user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Id");
                return RedirectToAction(nameof(Login));
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Email is already confirmed");
                return RedirectToAction(nameof(Login));
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var Link = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

            await _emailsender.SendEmailAsync(user.Email!, "Confirm your email", $"Please confirm your email by clicking on the following link: <a href='{Link}'>Confirm Email</a>");

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
      
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgetpasswordVM forgotPasswordVM,CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(forgotPasswordVM);
            }
            var user = await _userManager.FindByEmailAsync(forgotPasswordVM.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid Email");
                return View(forgotPasswordVM);
            }
            var Otps = await _otpRepository.GetAllAsync(e => e.UserId == user.Id, cancellationToken: cancellationToken, tracking: false);

            var existingOtp = Otps.Count(o =>(DateTime.UtcNow - o.CreatedAt).TotalHours < 24);
            if (existingOtp > 5)
            {
                ModelState.AddModelError(string.Empty, "You have exceeded the maximum number of OTP requests. Please try again later.");
                return View(forgotPasswordVM);
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var otpEntity = new Otps
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Otp = otp,
                ExpireAt = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow,
                Isvalid = true
            };
            await _otpRepository.AddAsync(otpEntity,cancellationToken);
            await _otpRepository.SaveChangesAsync(cancellationToken);

            await _emailsender.SendEmailAsync(user.Email!, "Ecomerse -_-_-.. Resend to password ",
             $"<h1>resent to Otp:{otp} to resent Your Acount </h1>");

            return RedirectToAction(nameof(ValidateOtp), new { userId = user.Id });
        }
        [HttpGet]
        public IActionResult ValidateOtp(string userId)
        {
            var validateOtpVM = new ValidateOtpVM
            {
                UserId = userId
            };
            return View(validateOtpVM);
        }
        [HttpPost]
        public async Task<IActionResult> ValidateOtp(ValidateOtpVM validateOtpVM,CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid Otp");
                return View(validateOtpVM);
            }
            var result = await _otpRepository.GetoneAsync(e => e.UserId == validateOtpVM.UserId && e.Otp == validateOtpVM.Otp && e.Isvalid,cancellationToken:cancellationToken);

            if (result == null || result.ExpireAt < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Invalid Otp");
                return View(validateOtpVM);
            }
            result.Isvalid = false;
             _otpRepository.Update(result);
            await _otpRepository.SaveChangesAsync(cancellationToken);

          return RedirectToAction(nameof(ResetPassword), new { userId = validateOtpVM.UserId });
        }
        [HttpGet]
        public IActionResult ResetPassword(string userId)
        {
            var resetPasswordVM = new ResetPasswordVM
            {
                UserId = userId
            };
            return View(resetPasswordVM);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View(resetPasswordVM);
            }
            var user = await _userManager.FindByIdAsync(resetPasswordVM.UserId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Id");
                return View(resetPasswordVM);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordVM.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(resetPasswordVM);
            }
            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }
            var user = await _userManager.FindByEmailAsync(loginVM.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "invalid user Name | Email OR password Invalid Login Attempt");
                return View(loginVM);
            }
            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "to many attemps, try again often 5 minAccount Locked Out");
                    return View(loginVM);
                }
                else if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View(loginVM);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "invalid user Name | Email OR password");
                    return View(loginVM);
                }
            }
           
            return RedirectToAction("Index", "Home", new { area = "User" });
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Try signing in with an external login
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl ?? "/");
            }

            // If the user cannot log in, try finding them by email
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name);
            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Create a new user if they do not exist
                    user = new Applicationuser
                    {
                        UserName = email,
                        FullName = username,
                        Email = email,
                        EmailConfirmed = true
                    };
                    var createUserResult = await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, DS.Role_User);

                    if (!createUserResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Error creating user.");
                        return RedirectToAction(nameof(Login));
                    }
                }

                // Ensure the external login is linked
                var existingLogins = await _userManager.GetLoginsAsync(user);
                var hasGoogleLogin = existingLogins.Any(l => l.LoginProvider == info.LoginProvider);

                if (!hasGoogleLogin)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Error linking external login.");
                        return RedirectToAction(nameof(Login));
                    }
                }

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl ?? "/");
            }

            return RedirectToAction(nameof(Login));
        }
    }
}
