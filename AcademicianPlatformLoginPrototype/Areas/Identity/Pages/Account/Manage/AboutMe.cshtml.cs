using AcademicianPlatform.Areas.Identity.Data;
using AcademicianPlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace AcademicianPlatform.Areas.Identity.Pages.Account.Manage
{
    public class AboutMeModel : PageModel
    {
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _environment;
		[TempData]
		public string StatusMessage { get; set; }
		public string? AboutMeText { get; set; }
		public AboutMeModel(
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
			[DisplayName("Hakkýmda")]
			public string AboutMeText { get; set; }
		}
		private void Load(ApplicationUser user)
		{
			Input = new InputModel
			{
				AboutMeText = user.AboutMeText
			};
		}
		public async Task<IActionResult> OnGetAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			Load(user);
			return Page();
		}
		public async Task<IActionResult> OnPostUpdateAboutMeAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			user.AboutMeText = Input.AboutMeText;
			await _userManager.UpdateAsync(user);
			StatusMessage = "Hakkýmda yazýnýz baþarýlý bir þekilde güncelleþtirildi!";
			return RedirectToPage("AboutMe");
		}
	}
}
