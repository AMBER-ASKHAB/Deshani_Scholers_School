using Application.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Areas.Auth.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace School_Management_System.Controllers
{
    [Area("Auth")]
    public class AuthController : Controller
    {
        private readonly SMSDbContext _dbcontext;
        private readonly IAuthServices _authService;
        public AuthController(SMSDbContext dbcontext, IAuthServices authService)
        {
            _dbcontext = dbcontext;
            _authService = authService;
        }

        //==Signup
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            bool success = _authService.Register(model.EmailAddress, model.Password);
            if (!success)
            {
                ModelState.AddModelError("EmailAddress", "Username already exists. Please choose another.");
                return View(model);
            }
            // Send activation email
            var activationLink = Url.Action("Login", "Auth", null, Request.Scheme);

            // var activationLink = Url.Action("ActivateAccount", "Auth", new { email = model.EmailAddress }, Request.Scheme);
            await _authService.SendActivationEmail(model.EmailAddress, activationLink);

            ViewBag.SignupSuccess = true;
            ViewBag.Email = model.EmailAddress;
            return View(model);
        }

        //=== Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = _authService.Login(model.Email, model.Password);

            if (result.Success && result.User != null)
            {
                var user = result.User;

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(7)
                                                  : DateTime.UtcNow.AddMinutes(30)
                };

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Ban_username), // email
            new Claim("BanId", user.BanId.ToString()),     // user id
            new Claim("Role", user.BanRole)                // role if needed
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties
                );

                return RedirectToAction("Application", "Applicants", new { area = "Applicants" });
            }

            ViewBag.Error = result.Message;
            return View(model);
        }


        // GET: Activate Account
        [HttpGet]
        public IActionResult ActivateAccount(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Message = "Invalid activation link.";
                return View();
            }

            var user = _dbcontext.Bandas.FirstOrDefault(u => u.Ban_username == email);
            if (user != null)
            {
                user.BanActive = true;
                _dbcontext.SaveChanges();
                ViewBag.Message = "Your account has been activated! You can now log in.";
            }
            else
            {
                ViewBag.Message = "Invalid activation link.";
            }

            return View();
        }

        //==Forgetpassword
        [HttpGet]
        public IActionResult Forgetpassword()
        {
            return View();
        }
        //==Forgetpassword
        [HttpPost]
        public async Task<IActionResult> Forgetpassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Message = "Please enter your email.";
                return View();
            }
            var user = await _dbcontext.Bandas.FirstOrDefaultAsync(u => u.Ban_username == email);
            if (user == null)
            {
                ViewBag.Message = "No account found with this email.";
                return View();
            }
            var resetToken = Guid.NewGuid().ToString();
            user.BanResetToken = resetToken;
            user.BanResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
            await _dbcontext.SaveChangesAsync();
            var resetLink = Url.Action("ResetPassword", "Auth", new { token = resetToken }, Request.Scheme);
            await _authService.SendResetPasswordEmail(email, resetLink);
            ViewBag.Message = "A password reset link has been sent to your email.";
            return View("Login");

        }
        //== Reset Password
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Message = "Invalid reset link.";
                return View();
            }

            var user = _dbcontext.Bandas.FirstOrDefault(u => u.BanResetToken == token && u.BanResetTokenExpiresAt > DateTime.UtcNow);
            if (user == null)
            {
                ViewBag.Message = "Reset link has expired or is invalid.";
                return View();
            }

            return View(new ResetPasswordViewModel { Token = token });
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (model.callingContext == "forgotPassword")
            {
                if (model.Password == "" || model.ConfirmPassword == "")
                {
                    return View(model);
                }
            }
            else
            {
                if (!ModelState.IsValid)
                {

                    return View(model);
                }
            }

            var user = _dbcontext.Bandas.FirstOrDefault(u => u.BanResetToken == model.Token && u.BanResetTokenExpiresAt > DateTime.UtcNow);
            if (user == null)
            {
                ViewBag.Message = "Reset link has expired or is invalid.";
                return View();
            }

            // Hash and update password
            user.BanPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            user.BanResetToken = null;
            user.BanResetTokenExpiresAt = null;
            await _dbcontext.SaveChangesAsync();

            TempData["Message"] = "Your password has been reset successfully. You can now login.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _dbcontext.Bandas.FirstOrDefaultAsync(u => u.Ban_username == User.Identity.Name);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Old_Password, user.BanPassword))
            {
                TempData["Message"] = "Old password is incorrect.";
                return View(model);
            }
            user.BanPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            await _dbcontext.SaveChangesAsync();
            TempData["Message"] = "Your password has been changed successfully.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var user = await _dbcontext.Bandas
              .FirstOrDefaultAsync(u => u.Ban_username == User.Identity.Name);

            if (user != null)
            {
                user.BanLastLoggedOffDateTime = DateTime.UtcNow;
                user.BanCurrentlyLogin = false;
                _dbcontext.Update(user);
                await _dbcontext.SaveChangesAsync();
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Auth");
        }

    }
}