using Microsoft.AspNetCore.Identity;
using AcademicianPlatform.Areas.Identity.Data;

namespace AcademicianPlatform.Data
{
    public class Seed
    {
        public static async Task SeedUsersAndRolesAsync(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                string user1 = "halis.altun@istun.edu.tr";

                var _user1 = await userManager.FindByEmailAsync(user1);
                if (_user1 == null)
                {
                    var newUser1 = new ApplicationUser()
                    {
                        UserName = "halis.altun",
                        FirstName = "Halis",
                        LastName = "Altun",
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(newUser1, "Halis123_");
                }

                string user2 = "nazli.tokatli@istun.edu.tr";

                var _user2 = await userManager.FindByEmailAsync(user2);
                if (_user2 == null)
                {
                    var newUser2 = new ApplicationUser()
                    {
                        UserName = "nazli.tokatli",
                        FirstName = "Nazlı",
                        LastName = "Tokatlı",
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(newUser2, "Nazli123_");
                }

            }
        }
    }
}