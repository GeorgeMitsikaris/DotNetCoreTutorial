using DotNetCoreTutorial.Models;
using DotNetCoreTutorial.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace DotNetCoreTutorial
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(_configuration.GetConnectionString("EmployeeDBConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options=> {
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 3;

                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddControllersWithViews();

            services.AddRazorPages();

            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();

            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();

            services.AddMvc(config => {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddAuthorization(config =>
            {
                config.AddPolicy("DeleteRolePolicy", policy =>
                {
                    policy.RequireClaim("Delete Role");
                });

                config.AddPolicy("EditRolePolicy", policy =>
                {
                    policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement());
                    //policy.RequireAssertion(handler =>
                    //{
                    //    return
                    //    (handler.User.IsInRole("Admin") && handler.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true")) ||
                    //    handler.User.IsInRole("Super Admin");
                    //});
                });

                config.AddPolicy("AdminRolePolicy", policy =>
                {
                    policy.RequireRole("Admin");
                });
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithRedirects("Error/{0}");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );

                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hosting environment " + env.EnvironmentName);
                //});
            });
        }
    }
}
