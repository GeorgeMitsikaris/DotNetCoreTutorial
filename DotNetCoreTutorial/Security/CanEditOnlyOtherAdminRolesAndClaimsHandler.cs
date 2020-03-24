using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetCoreTutorial.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CanEditOnlyOtherAdminRolesAndClaimsHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimsRequirement requirement)
        {
            var authFilterContext = httpContextAccessor.HttpContext;
            if (authFilterContext == null)
            {
                return Task.CompletedTask;
            }
            string adminIdBeingEdited = "";
            string loggedInAdminId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            string request = authFilterContext.Request.Path.Value.Split('/')[2];
            if(request=="ManageUserRoles" || request == "ManageUserClaims")
            {
                adminIdBeingEdited = authFilterContext.Request.QueryString.Value.Split('=')[1];
            }

            if(context.User.IsInRole("Admin") && context.User.HasClaim(c=>c.Type=="Edit Role" && c.Value=="true") && loggedInAdminId.ToLower() != adminIdBeingEdited.ToLower())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
