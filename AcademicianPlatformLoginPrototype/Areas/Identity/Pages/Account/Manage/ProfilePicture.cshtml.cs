using AcademicianPlatform.Areas.Identity.Data;
using AcademicianPlatform.Data;
using AcademicianPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.IO;

namespace AcademicianPlatform.Areas.Identity.Pages.Account.Manage
{
    public class ProfilePictureModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        [TempData]
        public string StatusMessage { get; set; }
        public string? ProfilePictureDirectory { get; set; }
        public ProfilePictureModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _environment = environment;
        }
		[BindProperty]
		public InputModel Input { get; set; }
        public class InputModel
        {
            [DisplayName("Profil Fotoðrafý")]
            public IFormFile ProfilePhoto { get; set; }
        }
        public async Task<IActionResult> OnGetAsync()
        {
            if(User.Identity?.Name == null)
            {
                return NotFound();
            }
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            ProfilePictureDirectory = currentUser.ProfilePhotoPath;
            return Page();
        }
        public async Task<IActionResult> OnPostUpdateProfilePictureAsync(IFormFile profilePicture)
        {
            Input.ProfilePhoto = profilePicture;
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            //Receive previous profile picture path
            string previousProfilePictureDirectory = user.ProfilePhotoPath ?? string.Empty;
            if(previousProfilePictureDirectory == null)
            {
                return NotFound();
            }
            //Let's delete the previous profile photo from the system
            try
            {
				System.IO.File.Delete(_environment.WebRootPath + previousProfilePictureDirectory);
			}
            catch (Exception e)
            {
                BadRequest(e.ToString());
            }
			//Now let's create new directory and copy the new profile picture to directory
			string profilePhotoName = Input.ProfilePhoto.FileName;
			string profilePhotoPath = Path.Combine(_environment.WebRootPath, "profilephotos", profilePhotoName);
			string relativeProfilePhotoPath = Path.GetRelativePath(_environment.WebRootPath, profilePhotoPath);
			using (var fileStream = new FileStream(profilePhotoPath, FileMode.Create))
			{
				await Input.ProfilePhoto.CopyToAsync(fileStream);
			}
			user.ProfilePhotoPath = "/" + relativeProfilePhotoPath;
            await _userManager.UpdateAsync(user);
            StatusMessage = "Profil fotoðrafýnýz baþarýyla güncellenmiþtir.";
			return RedirectToPage("ProfilePicture");
        }
    }
}
