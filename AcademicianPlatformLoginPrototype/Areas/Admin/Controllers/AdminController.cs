using AcademicianPlatform.Areas.Identity.Data;
using AcademicianPlatform.Data;
using AcademicianPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AcademicianPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        public AdminController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }
        public bool isAdmin = false;

        public IActionResult Index()
        {
            if(CheckIfAdmin() == false)
            {
                return NotFound();
            }
			var users = _context.Users.ToList();
            return View(users);
        }
        public IActionResult Support()
        {
			if (CheckIfAdmin() == false)
			{
				return NotFound();
			}
            IEnumerable<Ticket> tickets = _context.Tickets
                .OrderByDescending(p => p.TicketId)
                .ToList();
            return View(tickets);
		}
        public IActionResult EditMarqueeText()
        {
			if (CheckIfAdmin() == false)
			{
				return NotFound();
			}
            var marqueeText = _context.MarqueeText.FirstOrDefault(p => p.Id == 1);
            return View(marqueeText);
		}
        [HttpPost]
        public async Task<IActionResult> UpdateMarqueeText(string marqueeText)
        {
            if (CheckIfAdmin() == false)
            {
                return NotFound();
            }
            var currentMarqueeText = _context.MarqueeText.FirstOrDefault(p => p.Id == 1);
            if(currentMarqueeText != null)
            {
                currentMarqueeText.Text = marqueeText;
                _context.MarqueeText.Update(currentMarqueeText);
                await _context.SaveChangesAsync();
                return View("EditMarqueeText", currentMarqueeText);
            }
            else
            {
                    MarqueeText newMarqueeText = new MarqueeText();
                    newMarqueeText.Text = marqueeText;
                    _context.MarqueeText.Add(newMarqueeText);
                    await _context.SaveChangesAsync();
                    return View("EditMarqueeText", newMarqueeText);
            }

        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string deleteUserId)
        {
			if (CheckIfAdmin() == false)
			{
				return NotFound();
			}
			var userToDelete = _context.Users.FirstOrDefault(p => p.Id == deleteUserId);
            var announcementsOfDeletedUser = _context.Announcements.Where(p => p.AnnouncementSenderID == deleteUserId).ToList();
            if(announcementsOfDeletedUser.Count() != 0)
            {
                foreach(var item in announcementsOfDeletedUser)
                {
					_context.Announcements.Remove(item);
				}
            }
            if (userToDelete != null)
            {
                _context.Users.Remove(userToDelete);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ReplyToTicket(int replyTicketId,string reply, string userNameWhoReplies)
        {
            if (CheckIfAdmin() == false)
            {
                return NotFound();
            }
            var ticket = _context.Tickets.FirstOrDefault(p => p.TicketId == replyTicketId);
            ticket.TicketRespondContent = reply;
            ticket.TicketRespondSenderUserName = userNameWhoReplies;
            ticket.isResolved = true;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
            IEnumerable<Ticket> tickets = _context.Tickets
                .OrderByDescending(p => p.TicketId)
                .ToList();
            return View("Support",tickets);
        }
        [HttpPost]
        public IActionResult GetSpecificUser(string firstName, string lastName)
        {
			if (CheckIfAdmin() == false)
			{
				return NotFound();
			}
			var user = _context.Users.FirstOrDefault(p => p.FirstName.ToLower() == firstName.ToLower());
            if(user == null)
            {
                user = _context.Users.FirstOrDefault(p => p.LastName.ToLower() == lastName.ToLower());
            }
            if(user == null)
            {
                return NotFound();
            }
            var userAnnouncements = _context.Announcements
                .Where(a => a.AnnouncementSenderID == user.Id)
                .OrderByDescending(a => a.ID)
                .ToList();

            var fullName = user.FirstName + " " + user.LastName.ToUpper();
            var model = new AcademicianDetailsViewModel
            {
                Academian = user,
                /*
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
				ProfilePhotoPath = user.ProfilePhotoPath,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Department = user.Department,
				Title = user.Title,
				AboutMeText = user.AboutMeText,
				CVPath = user.CVPath,
				LastLogin = user.LastLogin.ToString(),
                */

				AcademianAnnouncements = userAnnouncements,
                FullName = fullName,

                
            };
            return View("EditUser", model);
        }
        public IActionResult EditUser()
        {
			if (CheckIfAdmin() == false)
			{
				return NotFound();
			}
			return View();
        }
        [HttpPost]
        public async Task<IActionResult> EditUserInformation(
            string Id = null,
            IFormFile profilePhoto = null,
            string title = null,
            string firstName = null,
            string lastName = null,
            string department = null,
            string phoneNumber = null,
            string aboutMe = null,
            IFormFile cv = null,
            string cvPath = null,
            string profilePhotoPath = null)
        {
			if (CheckIfAdmin() == false)
			{
				return NotFound();
			}
			var user = await _userManager.FindByIdAsync(Id);
            if(user == null)
            {
                return NotFound();
            }
            if(profilePhoto != null)
            {
                //Receive previous profile picture path
                string previousProfilePictureDirectory = user.ProfilePhotoPath ?? string.Empty;
                if (previousProfilePictureDirectory == null)
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
                string profilePhotoName = profilePhoto.FileName;
                string profilePhoto_Path = Path.Combine(_environment.WebRootPath, "profilephotos", profilePhotoName);
                string relativeProfilePhotoPath = Path.GetRelativePath(_environment.WebRootPath, profilePhoto_Path);
                using (var fileStream = new FileStream(profilePhoto_Path, FileMode.Create))
                {
                    await profilePhoto.CopyToAsync(fileStream);
                }
                profilePhotoPath = "/" + relativeProfilePhotoPath;
            }
            if(cv != null)
            {
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
                string CVName = cv.FileName;
                string CVPath = Path.Combine(_environment.WebRootPath, "cvs", CVName);
                string relativeCVPath = Path.GetRelativePath(_environment.WebRootPath, CVPath);
                using (var fileStream = new FileStream(CVPath, FileMode.Create))
                {
                    await cv.CopyToAsync(fileStream);
                }
                cvPath = "/" + relativeCVPath;
            }
            if (profilePhotoPath != null)
            {
                user.ProfilePhotoPath = profilePhotoPath;
            }
            if (title != null)
            {
                user.Title = title;
            }
            if (firstName != null)
            {
                user.FirstName = firstName;
            }
            if (lastName != null)
            {
                user.LastName = lastName;
            }
            if (department != null)
            {
                user.Department = department;
            }
            if (phoneNumber != null)
            {
                user.PhoneNumber = phoneNumber;
            }
            if (aboutMe != null)
            {
                user.AboutMeText = aboutMe;
            }
            if (cvPath != null)
            {
                user.CVPath = cvPath;
            }
            await _userManager.UpdateAsync(user);
            var userAnnouncements = _context.Announcements
                .Where(a => a.AnnouncementSenderID == user.Id)
                .OrderByDescending(a => a.ID)
                .ToList();

            var fullName = user.FirstName + " " + user.LastName.ToUpper();
            var model = new AcademicianDetailsViewModel
            {
                Academian=user,
                /*
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePhotoPath = user.ProfilePhotoPath,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                Title = user.Title,
                AboutMeText = user.AboutMeText,
                CVPath = user.CVPath,
                LastLogin = user.LastLogin.ToString(),
                */
                AcademianAnnouncements = userAnnouncements,
                FullName = fullName,
                
            };
            return View("EditUser", model);
        }
        [HttpPost]
        public async Task<IActionResult> EditAnnouncement(string AnnouncementTitle, string AnnouncementFaculty, string AnnouncementContent, int AnnouncementId)
        {
			if (CheckIfAdmin() == false)
			{
				return NotFound();
			}
			var announcementToEdit = _context.Announcements.FirstOrDefault(a => a.ID == AnnouncementId);
            if (announcementToEdit == null)
            {
                return NotFound();
            }
            announcementToEdit.AnnouncementTitle = AnnouncementTitle;
            announcementToEdit.AnnouncementFaculty = AnnouncementFaculty;
            announcementToEdit.AnnouncementContent = AnnouncementContent;
            await _context.SaveChangesAsync();
            return View("EditUser");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAnnouncement(int announcementId)
        {
			if (CheckIfAdmin() == false)
			{
				return NotFound();
			}
			var announcementToDelete = _context.Announcements.FirstOrDefault(a => a.ID == announcementId);
            Console.WriteLine(announcementToDelete);
            if (announcementToDelete == null)
            {
                return NotFound();
            }
            _context.Announcements.Remove(announcementToDelete);
            await _context.SaveChangesAsync();
            return View("EditUser");
        }
        public bool CheckIfAdmin()
        {
			string adminsContent = System.IO.File.ReadAllText(System.IO.Path.Combine(_environment.ContentRootPath, "External", "admins.json"));
			var admins = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(adminsContent) ?? throw new ArgumentNullException("Admin listesi bulunamadı!");
			List<string> emailsInAdmins = admins["emails"];
            List<string> adminUserNames = new List<string>();
            foreach(var email in emailsInAdmins)
            {
                adminUserNames.Add(TruncateEmail(email));
            }
            if (adminUserNames.Contains(User.Identity.Name))
            {
                isAdmin = true;
            }
            else
            {
                isAdmin = false;
            }
            return isAdmin;
		}
		static string TruncateEmail(string email)
		{
			int atIndex = email.IndexOf('@');
			if (atIndex != -1)
			{
				return email.Substring(0, atIndex);
			}
			return email; // Return the original email if "@" is not found
		}
	}
}
