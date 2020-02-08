using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreTutorial.Models;
using DotNetCoreTutorial.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCoreTutorial.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
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
    }
}