using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreTutorial.Models;
using DotNetCoreTutorial.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotNetCoreTutorial.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser { Email = model.Email, UserName = model.Email, City = model.City };
                var result = await userManager.CreateAsync(user, model.Password).ConfigureAwait(false);

                var token = await userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

                logger.Log(LogLevel.Warning, confirmationLink);

                if (result.Succeeded)
                {
                    if(signInManager.IsSignedIn(User) &&  User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }
                   
                ViewBag.ErrorTitle = "Registration Successful";
                ViewBag.ErrorMessage = "Before you can Login, please confirm your email, by clicking on the confirmation link we have emailed you";

                return View("Error");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync().ConfigureAwait(false);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.UserName).ConfigureAwait(false);

                if(user!=null && !user.EmailConfirmed && (await userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false)))
                {
                    ModelState.AddModelError("", "Email is not confirmed yet");
                    return View(model);
                }

                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false).ConfigureAwait(false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");

                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                }
            }
            return View(model);
        }

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);

            return user != null ? Json($"Email {email} is already in use") : Json(true);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId == null || token == null) 
            {
                return View("Index", "Home");
            }

            var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"The user id {userId} is invalid";
                return View("NotFound");
            }

            var result = await userManager.ConfirmEmailAsync(user, token).ConfigureAwait(false);

            if (result.Succeeded)
            {
                return View();
            }
            else
            {
                ViewBag.ErrorTitle = "The email cannot be confirmed";
                return View("Error");
            }
        }
    }
}