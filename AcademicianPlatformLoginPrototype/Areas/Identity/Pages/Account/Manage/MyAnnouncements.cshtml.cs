using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AcademicianPlatform.Areas.Identity.Data;
using AcademicianPlatform.Data;
using AcademicianPlatform.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicianPlatform.Areas.Identity.Pages.Account.Manage
{
    public class MyAnnouncementsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        [TempData]
        public string StatusMessage { get; set; }
        public List<Announcement> Announcements { get; set; }
        public MyAnnouncementsModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            Announcements = _context.Announcements.Where(a => a.AnnouncementSenderID == currentUser.Id).OrderByDescending(a => a.ID).ToList();
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(int announcementId)
        {
            var announcementToDelete = _context.Announcements.FirstOrDefault(a => a.ID == announcementId);
            Console.WriteLine(announcementToDelete);
            if (announcementToDelete == null)
            {
                return NotFound();
            }
            _context.Announcements.Remove(announcementToDelete);
            await _context.SaveChangesAsync();
            return RedirectToPage("MyAnnouncements");
        }
        public async Task<IActionResult> OnPostEditAsync(string AnnouncementTitle,string AnnouncementFaculty, string AnnouncementContent, int AnnouncementId)
        {
            var announcementToEdit = _context.Announcements.FirstOrDefault(a => a.ID == AnnouncementId);
            if(announcementToEdit == null)
            {
                return NotFound();
            }
            announcementToEdit.AnnouncementTitle = AnnouncementTitle;
            announcementToEdit.AnnouncementFaculty = AnnouncementFaculty;
            announcementToEdit.AnnouncementContent = AnnouncementContent;
            await _context.SaveChangesAsync();
            return RedirectToPage("MyAnnouncements");
        }
    }
}
