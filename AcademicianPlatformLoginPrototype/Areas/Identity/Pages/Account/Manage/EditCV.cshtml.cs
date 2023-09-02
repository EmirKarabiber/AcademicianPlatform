using AcademicianPlatform.Areas.Identity.Data;
using AcademicianPlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace AcademicianPlatform.Areas.Identity.Pages.Account.Manage
{
    public class EditCVModel : PageModel
    {
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _environment;
		[TempData]
		public string StatusMessage { get; set; }
		public string? CVDirectory { get; set; }
		public EditCVModel(
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
			[DisplayName("CV")]
			public IFormFile CV { get; set; }
		}
		public async Task<IActionResult> OnGetAsync()
		{
			if (User.Identity?.Name == null)
			{
				return NotFound();
			}
			var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
			CVDirectory = currentUser.CVPath;
			return Page();
		}
		public async Task<IActionResult> OnPostUpdateCVAsync(IFormFile cv)
		{
			Input.CV = cv;
			var user = await _userManager.FindByNameAsync(User.Identity?.Name);
			//Receive previous profile picture path
			string previousCVDirectory = user.CVPath ?? string.Empty;
			if (previousCVDirectory == null)
			{
				return NotFound();
			}
			//Let's delete the previous profile photo from the system
			try
			{
				System.IO.File.Delete(_environment.WebRootPath + previousCVDirectory);
			}
			catch (Exception e)
			{
				BadRequest(e.ToString());
			}
			//Now let's create new directory and copy the new profile picture to directory
			string CVName = Input.CV.FileName;
			string CVPath = Path.Combine(_environment.WebRootPath, "cvs", CVName);
			string relativeCVPath = Path.GetRelativePath(_environment.WebRootPath, CVPath);
			using (var fileStream = new FileStream(CVPath, FileMode.Create))
			{
				await Input.CV.CopyToAsync(fileStream);
			}
			user.CVPath = "/" + relativeCVPath;
			await _userManager.UpdateAsync(user);
			StatusMessage = "CVniz baþarýyla güncellenmiþtir.";
			return RedirectToPage("EditCV");
		}
		public async Task<IActionResult> OnPostUploadCVAsync(IFormFile cv)
		{
			Input.CV = cv;
			var user = await _userManager.FindByNameAsync(User.Identity?.Name);
			//Now let's create new directory and copy the new profile picture to directory
			string CVName = Input.CV.FileName;
			string CVPath = Path.Combine(_environment.WebRootPath, "cvs", CVName);
			string relativeCVPath = Path.GetRelativePath(_environment.WebRootPath, CVPath);
			using (var fileStream = new FileStream(CVPath, FileMode.Create))
			{
				await Input.CV.CopyToAsync(fileStream);
			}
			user.CVPath = "/" + relativeCVPath;
			await _userManager.UpdateAsync(user);
			StatusMessage = "CVniz baþarýyla sisteme yüklenmiþtir.";
			return RedirectToPage("EditCV");
		}
	}
}

