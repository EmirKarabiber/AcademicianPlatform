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
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IUserStore<ApplicationUser> userStore, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userStore = userStore;
            _userManager = userManager;
        }
        [Authorize]
        public IActionResult Index()
        {

            //	var announcements = _context.Announcements?.ToList();
            var announcements = _context.Announcements?.OrderByDescending(a => a.ID).ToList();  //duyuruları tersten sıralama eklendi
            return View(announcements);
        }
        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize]
        public IActionResult NewAnnouncement()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> PostNewAnnouncement(string announcementTitle, string announcementContent, string senderName)
        {
            var user = await _userManager.FindByNameAsync(senderName);
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
        [Authorize]
        public async Task<IActionResult> DeleteAnnouncement(int announcementID)
        {
            var announcementToDelete = _context.Announcements.Find(announcementID);
            if (announcementToDelete != null)
            {
                _context.Announcements.Remove(announcementToDelete);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Authorize]

        public IActionResult AnnouncementDetails([FromRoute(Name = "ID")] int announcementID)
        {
            // Mail bilgilerini hazırla
            string recipientEmail = "destek@example.com";
            string subject = "Konu";    //şu kısımlar bir şekilde sayfadan çekilecek
            string body = "İçerik";

            // Mailto linki oluştur
            string mailtoLink = $"mailto:{recipientEmail}?subject={subject}&body={body}";

            // Mailto linkini View'e taşı
            ViewBag.MailtoLink = mailtoLink;


            var announcement = _context.Announcements.FirstOrDefault(a => a.ID == announcementID);  //duyuruları başka ekranda açma
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }
        [Authorize]

        public async Task<IActionResult> MyAnnouncoments()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            var userAnnouncements = _context.Announcements
                .Where(a => a.AnnouncementSenderID == user.Id)
                .OrderByDescending(a => a.ID)
                .ToList();

            return View(userAnnouncements);
        }
        //----

    }
}