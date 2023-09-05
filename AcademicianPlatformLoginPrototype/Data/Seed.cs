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
                        Id="halisId",
                        UserName = "halis.altun",
                        FirstName = "Halis",
                        LastName = "Altun",
                        Email = user1,
                        Department = "Yazılım Mühendisliği",
                        Title = "Prof. Dr.",
                        AboutMeText = "Yazılım mühendisliği bölüm başkanı.",
                        ProfilePhotoPath = null,
                        CVPath = null,
                    };
                    await userManager.CreateAsync(newUser1, "123");
                    await userManager.UpdateAsync(newUser1);
                }

                string user2 = "nazli.tokatli@istun.edu.tr";

                var _user2 = await userManager.FindByEmailAsync(user2);
                if (_user2 == null)
                {
                    var newUser2 = new ApplicationUser()
                    {
                        Id="nazliId",
                        UserName = "nazli.tokatli",
                        FirstName = "Nazlı",
                        LastName = "Tokatlı",
                        Email = user2,
                        Department = "Bilgisayar Mühendisliği",
                        Title = "Dr. Öğr. Üyesi",
                        AboutMeText = "Bilgisayar mühendisliği bölüm başkanı.",
                        ProfilePhotoPath = null,
                        CVPath = null,
                    };
                    await userManager.CreateAsync(newUser2, "123");
                    await userManager.UpdateAsync(newUser2);
                }
            }
        }
    }
}