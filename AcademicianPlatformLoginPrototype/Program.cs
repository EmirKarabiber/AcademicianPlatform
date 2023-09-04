using AcademicianPlatform.Areas.Identity.Data;
using AcademicianPlatform.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data;

namespace AcademicianPlatform
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));
			builder.Services.TryAddScoped<MicrosoftSignInManager<ApplicationUser>>();
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
				.AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
                microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
				microsoftOptions.SignInScheme = "Identity.External";
			});
            builder.Services.AddControllersWithViews();
			var app = builder.Build();
            if (args.Length == 1 && args[0].ToLower() == "seeddata")
            {
                await Seed.SeedUsersAndRolesAsync(app);
				return;
            }
            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute(
                //name: "Admin",
                //pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}"
                //);
                endpoints.MapAreaControllerRoute(
				name: "Admin",
				areaName: "Admin",
				pattern: "Admin/{controller=Admin}/{action=Index}/{id?}");
            });
            app.MapRazorPages();

			app.Run();
		}
	}
}