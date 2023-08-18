using AcademicianPlatformLoginPrototype.Areas.Identity.Data;
using AcademicianPlatformLoginPrototype.Data;
using AcademicianPlatformLoginPrototype.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AcademicianPlatformLoginPrototype.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly IUserStore<ApplicationUser> _userStore;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IUserStore<ApplicationUser> userStore)
		{
			_logger = logger;
			_context = context;
			_userStore = userStore;
		}
		[Authorize]
		public IActionResult Index()
		{
			var announcements = _context.Announcements?.ToList();
			return View(announcements);
		}
		[Authorize]
		public IActionResult Privacy()
		{
			return View();
		}
		public IActionResult NewAnnouncement()
		{
			return View();
		}
		public async Task<IActionResult> PostNewAnnouncement(string announcementTitle, string announcementContent, string senderName)
		{
			var user = await _userStore.FindByNameAsync(senderName,CancellationToken.None);
			Announcement announcement = new Announcement()
			{
				AnnouncementTitle = announcementTitle,
				AnnouncementContent = announcementContent,
				AnnouncementSentDate = DateTime.Now,
				AnnouncementSenderID = user.Id
			};
			await _context.Announcements.AddAsync(announcement);
			await _context.SaveChangesAsync();
			return RedirectToAction("Index");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}