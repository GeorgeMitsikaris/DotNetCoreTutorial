using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCoreTutorial.Models;
using DotNetCoreTutorial.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetCoreTutorial.Controllers
{
    //[Authorize(Policy = "AdminRolePolicy")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        public AdministrationController(RoleManager<IdentityRole> roleManager,
                                        UserManager<ApplicationUser> userManager,
                                        ILogger<AdministrationController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole { Name = model.RoleName };
                var result = await roleManager.CreateAsync(identityRole).ConfigureAwait(false);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id).ConfigureAwait(false);
            if (role == null)
            {
                ViewBag.Message = $"The role with id {id} cannot be found";
                return View("NotFound");
            }
            else
            {
                EditRoleViewModel model = new EditRoleViewModel
                {
                    Id = role.Id,
                    RoleName = role.Name
                };

                foreach (var user in userManager.Users.ToList())
                {
                    if (await userManager.IsInRoleAsync(user, role.Name).ConfigureAwait(false))
                    {
                        model.Users.Add(user.UserName);
                    }
                }
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id).ConfigureAwait(false);
            if (role == null)
            {
                ViewBag.Message = $"The role with id {id} cannot be found";
                return View("NotFound");
            }
            else
            {
                try
                {
                    var result = await roleManager.DeleteAsync(role).ConfigureAwait(false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View("ListRoles");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ViewBag.ErrorTitle = $"The role {role.Name} is in use";
                    ViewBag.ErrorMessage = "If you want to delete this role, first delete the users who have this role";
                    logger.LogError($"error deleting role {ex}");
                    return View("Error");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id).ConfigureAwait(false);
            if (role == null)
            {
                ViewBag.Message = $"The role with id {model.Id} cannot be found";
                return RedirectToAction("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role = await roleManager.FindByIdAsync(roleId).ConfigureAwait(false);

            if (role == null)
            {
                ViewBag.Message = $"The role with id {roleId} cannot be found";
                return RedirectToAction("NotFound");
            }

            var model = new List<EditUserInRoleViewModel>();

            foreach (var user in userManager.Users.ToList())
            {
                var editUsersInRole = new EditUserInRoleViewModel();
                editUsersInRole.UserId = user.Id;
                editUsersInRole.UserName = user.UserName;
                editUsersInRole.IsInRole = await userManager.IsInRoleAsync(user, role.Name).ConfigureAwait(false);
                model.Add(editUsersInRole);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<EditUserInRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId).ConfigureAwait(false);

            if (role == null)
            {
                ViewBag.Message = $"The role with id {roleId} cannot be found";
                return RedirectToAction("NotFound");
            }

            if (model == null)
            {
                return RedirectToAction("NotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId).ConfigureAwait(false);
                IdentityResult result = null;

                if (model[i].IsInRole && !await userManager.IsInRoleAsync(user, role.Name).ConfigureAwait(false))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name).ConfigureAwait(false);
                }
                else if (!model[i].IsInRole && await userManager.IsInRoleAsync(user, role.Name).ConfigureAwait(false))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name).ConfigureAwait(false);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i <= model.Count - 1)
                    {
                        continue;
                    }
                    else
                    {
                        return RedirectToAction("EditRole", new { id = role.Id });
                    }
                }
            }
            return RedirectToAction("EditRole", new { id = role.Id });
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            return View(userManager.Users.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id).ConfigureAwait(false);

            if (user == null)
            {
                ViewBag.Message = $"The user with id {id} cannot be found";
                return RedirectToAction("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            var claims = await userManager.GetClaimsAsync(user).ConfigureAwait(false);

            EditUserViewModel model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                City = user.City,
                Claims = claims.Select(c => c.Type + ": " + c.Value).ToList(),
                Roles = roles
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id).ConfigureAwait(false);

            if (user == null)
            {
                ViewBag.Message = $"The user with id {model.Id} cannot be found";
                return RedirectToAction("NotFound");
            }
            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.City = model.City;

                var result = await userManager.UpdateAsync(user).ConfigureAwait(false);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id).ConfigureAwait(false);

            if (user == null)
            {
                ViewBag.Message = $"The user with id {id} cannot be found";
                return NotFound();
            }

            var result = await userManager.DeleteAsync(user).ConfigureAwait(false);

            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View("ListUsers");
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.UserId = userId;

            var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user == null)
            {
                ViewBag.Message = $"The user with id {userId} cannot be found";
                return NotFound();
            }

            var model = new List<UserRolesViewModel>();

            foreach (var role in roleManager.Roles.ToList())
            {
                var userRoleViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                if (await userManager.IsInRoleAsync(user, role.Name).ConfigureAwait(false))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user == null)
            {
                ViewBag.Message = $"The user with id {userId} cannot be found";
                return NotFound();
            }

            var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            var result = await userManager.RemoveFromRolesAsync(user, roles).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove roles from user");
                return View(model);
            }

            result = await userManager.AddToRolesAsync(user, model.Where(m => m.IsSelected).Select(m => m.RoleName)).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add roles to user");
                return View(model);
            }
            return RedirectToAction("EditUser", new { id = userId });
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(string id)
        {
            var user = await userManager.FindByIdAsync(id).ConfigureAwait(false);

            if (user == null)
            {
                ViewBag.Message = $"The user with id {id} cannot be found";
                return NotFound();
            }

            var existingUserClaims = await userManager.GetClaimsAsync(user).ConfigureAwait(false);

            var model = new UserClaimsViewModel
            {
                UserId = id
            };

            foreach (var claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type,
                };

                if (existingUserClaims.Any(c => c.Type == claim.Type && c.Value == "true"))
                {
                    userClaim.IsSelected = true;
                }

                model.Claims.Add(userClaim);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId).ConfigureAwait(false);

            if (user == null)
            {
                ViewBag.Message = $"The user with id {model.UserId} cannot be found";
                return NotFound();
            }

            var claims = await userManager.GetClaimsAsync(user).ConfigureAwait(false);
            var result = await userManager.RemoveClaimsAsync(user, claims).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove existing user claims");
                return View(model);
            }

            result = await userManager.AddClaimsAsync(user, model.Claims.Select(c => new Claim(c.ClaimType, c.IsSelected?"true":"false"))).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add user claims");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = model.UserId });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}