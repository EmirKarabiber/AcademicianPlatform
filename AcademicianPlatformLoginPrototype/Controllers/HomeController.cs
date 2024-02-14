using AcademicianPlatform.Areas.Identity.Data;
using AcademicianPlatform.Data;
using AcademicianPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Diagnostics;
using System.Net.Mail;
using MailKit.Security;
using MailKit.Net.Smtp;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.AspNetCore.Authentication;

namespace AcademicianPlatform.Controllers
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

		//Index sayfasında sadece son 1 ayki duyuruların gözükmesini sağla
		DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);

		private static List<object>? notificationList = new List<object>();
		private static List<Announcement>? newsPageAnnouncementsList = new List<Announcement>();
		//static yapısı testler sonrasında değiştirilebilir.

		[Authorize]
		public async Task<IActionResult> Index()
		{
			if (User.Identity != null && User.Identity.IsAuthenticated)
			{
				var user = await _userManager.FindByNameAsync(User.Identity.Name);

				if (user == null)
				{
					await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
					return RedirectToPage("/Identity/Login");
				}

				var lastLogin = user.LastLogin;
				var model = await GetIndexModel("Tüm Fakülteler", user, lastLogin);

				user.LastLogin = DateTime.Now;

				await _userManager.UpdateAsync(user);

				return View(model);
				
			}
			else
			{
				// User is not authenticated, handle accordingly (e.g., redirect to login page)
				//eturn RedirectToAction("Login", "Account");
				return RedirectToPage("/Identity/Login");
			}
		}

		
		public async Task<IndexModel> GetIndexModel(string announcementFaculty, ApplicationUser user, DateTime? lastLogin)
		{
			List<Announcement>? allAnnouncements;
			List<Announcement>? specialAnnouncements;

			if (announcementFaculty == "Tüm Fakülteler")
			{
				 allAnnouncements = await _context.Announcements
				.Where(a => a.AnnouncementSentDate >= oneMonthAgo)
				.OrderByDescending(a => a.ID)
				.ToListAsync();

				 specialAnnouncements = await _context.Announcements
					.Where(a => a.AnnouncementSentDate >= oneMonthAgo && a.AnnouncementSpecial == true)
					.OrderByDescending(a => a.ID)
					.ToListAsync();
			}
			else
			{
				 allAnnouncements = await _context.Announcements
				.Where(a => a.AnnouncementSentDate >= oneMonthAgo &&
					(a.AnnouncementFaculty == announcementFaculty || a.AnnouncementFaculty == "Tüm Fakülteler"))
				.OrderByDescending(a => a.ID)
				.ToListAsync();

				specialAnnouncements = await _context.Announcements
					.Where(a => a.AnnouncementSentDate >= oneMonthAgo && a.AnnouncementSpecial == true &&
						(a.AnnouncementFaculty == announcementFaculty || a.AnnouncementFaculty == "Tüm Fakülteler"))
					.OrderByDescending(a => a.ID)
					.ToListAsync();
			}

			var newComments = await _context.Comments
				.Where(c => c.DatePosted > lastLogin && c.Announcement.AnnouncementSenderID == user.Id)
				.OrderByDescending(c => c.DatePosted)
				.ToListAsync();

			var followers = await _context.Follows
				.Where(f => f.FollowedUserId == user.Id && f.FollowDate > lastLogin)
				.OrderByDescending(f => f.FollowDate)
				.ToListAsync();

			if (notificationList.Count == 0 || (notificationList.Count > 0 && (newComments.Count > 0 || followers.Count > 0)))	//combined list içerisinde eleman yoksa böyle dolduracak. içinde eleman olunca doldurmaz zaten
			{
				
				var followerUsersOrderedByDate = new List<FollowModelForIndexModel>();
				foreach (var follow in followers)
				{
					var followerUser = await _context.Users
						.Where(u => u.Id == follow.FollowerId)
						.FirstOrDefaultAsync();

					if (followerUser != null)
					{
						followerUsersOrderedByDate.Add(new FollowModelForIndexModel
						{
							NewFollowerUsers = followerUser,
							NewFollowersFollow = follow,
							followerid = followerUser.Id
						});
					}

				}
				
					notificationList.AddRange(newComments);
					notificationList.AddRange(followerUsersOrderedByDate);
					notificationList = notificationList.OrderByDescending(item =>
					{
						if (item is Comment comment)
						{
							return comment.DatePosted;
						}
						else if (item is FollowModelForIndexModel followerUser)
						{
							return followerUser.NewFollowersFollow.FollowDate;
						}
						return DateTime.MinValue;
					}).ToList();
				
			}

			var model = new IndexModel
			{
				AllAnnouncement = allAnnouncements,
				SpecialAnnouncement = specialAnnouncements,
				NotificationList = notificationList
			};

			return model;
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
		public async Task<IActionResult> PostNewAnnouncement(string announcementTitle, string announcementContent, string senderName, string announcementFaculty, bool sendToAll, bool isSpeacialAnnouncement)
		{
			var user = await _userManager.FindByNameAsync(senderName);
			Announcement announcement = new Announcement()
			{
				AnnouncementTitle = announcementTitle,
				AnnouncementContent = announcementContent,
				AnnouncementSentDate = DateTime.Now,
				AnnouncementSenderID = user.Id,
				AnnouncementFaculty = announcementFaculty,
				AnnouncementSpecial = isSpeacialAnnouncement
			};
			await _context.Announcements.AddAsync(announcement);
			await _context.SaveChangesAsync();
			if (sendToAll)
			{
				return RedirectToAction("SendEmailAll", announcement);
			}
			else
			{
				return RedirectToAction("Index");
			}
		}


		public async Task<IActionResult> SendEmailAll(Announcement announcement)
		{
			//Sisteme kayıtlı tüm maillere mail göndermeye yarıyor.

			var usersWithEmails = await _userManager.Users  // email adresi girili hesapları listeliyor
				.Where(u => !string.IsNullOrEmpty(u.Email)) 
				.ToListAsync();

			var sentEmails = new List<string>();	//mail giden hesapları daha sonra göstermek için

			var user = await _userManager.FindByNameAsync(User.Identity.Name);

			foreach (var userWithEmail in usersWithEmails)
			{
				var recipientEmail = userWithEmail.Email;
				var message = new MimeMessage();
				var bodyBuilder = new BodyBuilder();

				//burada gönderen, alıcı gibi ifadeler olmasa da olur, mesela gönderen yerine direkt {title first last} gibi fullname yapısı kullanılabilir.
				message.From.Add(new MailboxAddress("Gönderen : " + user.Email, user.Email));
				message.To.Add(new MailboxAddress("Alıcı : " + recipientEmail, recipientEmail));
				string userFullName = user.Title + " " + user.FirstName + " " + user.LastName;
				message.Subject = "AkademISTUN'de Yeni Duyuru Paylaşıldı :" + announcement.AnnouncementTitle + " Hakkında";

				//Not: bu yazıya style etiketi ile css tasarımı da yapılabilir lakin her mail sistemi kabul etmeyebileceğinden biraz sonraya bırakılabilir.
				bodyBuilder.HtmlBody ="<html><head><style>"+
				  "body {font-family: Arial, sans-serif;background-color: #f4f4f4;margin: 0;padding: 0;display: flex;justify-content: center;align-items: center;height: 100vh; }" +
				  ".container {background-color: #fff; border-radius: 8px;box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); margin:40px;padding: 20px;border:2px solid #d3d3d3}" +
				  ".ust {color: #85150d;text-align: center;}"+
				  "p {margin: 10px 0;}"+
				  "a {text-decoration: none;color: #85150d;}" +
				  "</style></head><body>" +
				  "<div class='container'><h2 class='ust'>"+userFullName+"</h2>"+"<h3 class='ust'>Yeni Duyuru Yayınladı</h3>"+
				  "<p><strong>"+user.Department+"</strong> departmanındaki <strong>"+userFullName+"</strong>, <strong>"+announcement.AnnouncementTitle+"</strong> başlıklı bir yeni duyuru yayınladı. Duyurunun içeriği aşağıdaki gibidir:</p><h4>"+announcement.AnnouncementTitle+"</h4>"+
				  "<p>"+announcement.AnnouncementContent+"</p><br>" +
				  "<p>Daha fazla ayrıntıya ulaşmak için <a href='https://localhost:7111/Home/AnnouncementDetails/" + announcement.ID + "'>bu bağlantıya</a> tıklayabilir ve duyuruyu inceleyebilirsiniz.</p><br><hr>" +
				  "<a href='https://localhost:7111/'>AkademISTUN</a></div></body></html>";
				message.Body = bodyBuilder.ToMessageBody();

				// E-posta gönderme işlemi için SMTP istemcisini kullanma
				using (var client = new MailKit.Net.Smtp.SmtpClient())
				{
					// SMTP sunucusuna bağlanma
					await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
					// Kimlik doğrulama
					await client.AuthenticateAsync("[redacted]", "[eyiyoklvmbrnqfbw]");
					// E-postayı gönderme
					await client.SendAsync(message);
					// SMTP sunucusundan çıkma
					await client.DisconnectAsync(true);
				}
				sentEmails.Add(recipientEmail);
			}

			var emailAddresses = sentEmails.ToArray();
			var recipientEmailsString = string.Join(", ", emailAddresses);
			//hangi maillere gittiğini gösterme amaçlı sayfa tasarımı

			var emailModel = new EmailViewModel
			{
				SenderEmail = user.Email,
				RecipientEmail = recipientEmailsString, // Set the list of recipient email addresses in the model
				Subject = "Yeni Duyuru: " + announcement.AnnouncementTitle,
				Body = announcement.AnnouncementContent
			};
			TempData["Message"] = "E-posta başarıyla gönderildi!";
			return RedirectToAction("EmailSenderResult", emailModel);
		}


		public IActionResult DeleteAnnouncement(int announcementID)
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

			var announcement = _context.Announcements
				.Include(a => a.Comments) // Yorumları dahil et
				.FirstOrDefault(a => a.ID == announcementID);

			if (announcement == null)
			{
				return NotFound();
			}

			var announcementViewModel = new AnnouncementViewModel
			{
				Announcement = announcement,

			};

			/*
			// Mail bilgilerini hazırla
			string recipientEmail = "destek@example.com";
			string subject = "Konu";    //şu kısımlar bir şekilde sayfadan çekilecek
			string body = "İçerik";
			// Mailto linki oluştur
			string mailtoLink = $"mailto:{recipientEmail}?subject={subject}&body={body}";
			// Mailto linkini View'e taşı
			ViewBag.MailtoLink = mailtoLink;
			*/

			return View(announcementViewModel);
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



		[Authorize]
		public async Task<IActionResult> EmailSender([FromRoute(Name = "ID")] int announcementID)
		{
			// Mevcut kullanıcıyı bul
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if(user== null)
			{
				return NotFound();	// Kullanıcı null ise 404 döndür
			}

			// Gönderici olarak kullanıcının e-posta adresini al
			string senderEmail = user.Email;

			// Belirtilen duyuruyu veritabanından bul
			Announcement announcement = _context.Announcements.FirstOrDefault(a => a.ID == announcementID);
			if (announcement == null)
			{
				return NotFound(); // Duyuru bulunamazsa 404 (Not Found) döndür
			}

			// Duyurunun sahibini veritabanından bul
			var announcementOwner = await _userManager.FindByIdAsync(announcement.AnnouncementSenderID);

			// E-posta gönderimi için kullanılacak modeli oluştur
			EmailViewModel model = new EmailViewModel
			{
				SenderEmail = senderEmail, // Gönderici e-posta adresi
				Subject = announcement.AnnouncementTitle.ToString() + " Hk.", // E-posta konusu
				RecipientEmail = announcementOwner.Email, // Alıcı e-posta adresi (duyuru sahibi)
				Body = null, // E-posta içeriği (varsayılan olarak boş bırakıldı)
				AnnouncementIDForEmail = announcementID
			};

			// Oluşturulan modeli view'e taşı ve e-posta gönderim sayfasını görüntüle
			return View(model);
		}




		[HttpPost]
		public async Task<IActionResult> SendEmail([FromForm] EmailViewModel model)
		{
			if (ModelState.IsValid)
			{
				// Oluşturulan email mesajı
				var message = new MimeMessage();
				message.From.Add(new MailboxAddress("Gönderen : " + model.SenderEmail, model.SenderEmail));
				message.To.Add(new MailboxAddress("Alıcı : " + model.RecipientEmail, model.RecipientEmail));
				message.Subject = model.Subject;

				var bodyBuilder = new BodyBuilder();
				bodyBuilder.HtmlBody = model.Body;
				message.Body = bodyBuilder.ToMessageBody();

				message.ReplyTo.Add(new MailboxAddress("Yanıt Adresi : " + model.SenderEmail, model.SenderEmail));

				// E-posta gönderme işlemi için SMTP istemcisini kullanma
				using (var client = new MailKit.Net.Smtp.SmtpClient())
				{
					// SMTP sunucusuna bağlanma
					await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

					// Kimlik doğrulama
					await client.AuthenticateAsync("platformacademician@gmail.com", "eyiyoklvmbrnqfbw");
					//yeni bir mail hesabı açtım , oradan gerekli şifreyi aldım lakin anlık olarak gereksiz mailler penceresine yolluyor maili

					// E-postayı gönder
					await client.SendAsync(message);

					// SMTP sunucusundan çıkma
					await client.DisconnectAsync(true);
				}

				TempData["Message"] = "E-posta başarıyla gönderildi!";
				return RedirectToAction("EmailSenderResult", model);
			}
			return View("EmailSender", model);
		}

		public IActionResult EmailSenderResult(EmailViewModel model)
		{
			return View(model);
		}

		//index sayfasındaki duyuruların sadece belirli bir fakülteye göre kategori edilmiş şekilde göster
		[Authorize]
		public async Task<IActionResult> IndexFaculty([FromQuery] string announcementFaculty)
		{
			// Aktif kullanıcı null ise Login sayfasına yönlendir.
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null)
			{
				await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
				return RedirectToPage("/Identity/Login");
			}

			//fakülte değeri null değilse, fakülteyle alakalı duyuru ve özel duyuruları modele kaydet
			if (!string.IsNullOrEmpty(announcementFaculty))
			{
				var model = await GetIndexModel(announcementFaculty, user, user.LastLogin);

				return View("Index", model);
			}

			return RedirectToAction("Index");
		}


		public IActionResult Academians(string academianDepartment)
		{
			var DepartmentUsers = new AcademicianWithDepartment
			{
				Department = academianDepartment
			};
			if (academianDepartment == "Sağlık Meslek")
			{
				var academians = _userManager.Users.Where(u => u.Department == "Anestezi" ||
				u.Department == "Ağız ve Diş Sağlığı" ||
				u.Department == "İlk ve Acil Yardım" ||
				u.Department == "Tıbbi Görüntüleme Teknikleri" ||
				u.Department == "Tıbbi Laboratuvar Teknikleri").ToList();
				DepartmentUsers.Users = academians;

			}
			else if (academianDepartment == "Yabancı Diller")
			{
				var academians = _userManager.Users.Where(u => u.Department == "İngilizce Mütercim-Tercümanlık" ||
				u.Department == "İngilizce Hazırlık Birimi" ||
				u.Department == "Ortak Yabancı Dil Dersleri Birimi").ToList();
				DepartmentUsers.Users = academians;

			}
			else
			{
				var academians = _userManager.Users.Where(u => u.Department == academianDepartment).ToList();
				DepartmentUsers.Users = academians;
			}

			return View(DepartmentUsers);
		}




		public async Task<IActionResult> AcademicianDetails(string id)
		{
			//Akademisyenlerin profil sayfaları için

			//AcademianDetails sayfasında akademisyenlerin duyurularının da gözükmesini sağla
			var academician = _userManager.Users.FirstOrDefault(u => u.Id == id);
			var AcademianAnnouncements = _context.Announcements
				.Where(a => a.AnnouncementSenderID == id)
				.OrderByDescending(a => a.ID)
				.ToList();
			

			var user = await _userManager.FindByNameAsync(User.Identity.Name);

			// AcademianDetails sayfasında, Aktif kullanıcı akademisyeni takip edip etmediğine bağlı buton görünümünü ayarlamak için
			string followerId = user.Id;
			string followingId = id;
			var follow = _context.Follows
				.FirstOrDefault(f => f.FollowerId == followerId && f.FollowedUserId == followingId);
			var isFollowing = (follow != null);

			var FullName = academician.FirstName + " " + academician.LastName.ToUpper();

			var viewModel = new AcademicianDetailsViewModel
			{
				Academian = academician,

				/*
				 * UserId = academician.Id,
				 * UserName = academician.UserName,
				 * Email = academician.Email,
				 * PhoneNumber = academician.PhoneNumber,
				 * ProfilePhotoPath = academician.ProfilePhotoPath,
				 * Department = academician.Department,
				 * Title = academician.Title,
				 * AboutMeText = academician.AboutMeText,
				 * CVPath = academician.CVPath,
				 * LastLogin = academician.LastLogin.ToString(),
				 */

				FullName = FullName,
				AcademianAnnouncements = AcademianAnnouncements,
				IsCurrentUser = (User.Identity.Name == academician.UserName),
				IsFollowing = isFollowing

			};

			return View(viewModel);
		}
		[HttpPost]
		public async Task<IActionResult> FollowUser(string userIdToFollow)
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

			if (currentUser != null)
			{
				// Kullanıcıyı takip etme işlemi
				var follow = new Follow
				{
					FollowerId = currentUser.Id,
					FollowedUserId = userIdToFollow
				};

				_context.Follows.Add(follow);
				await _context.SaveChangesAsync();
			}

			// İşlem tamamlandığında kullanıcıyı doğru sayfaya yönlendirin
			return RedirectToAction("AcademicianDetails", new { id = userIdToFollow });
		}
		[HttpPost]
		public async Task<IActionResult> UnfollowUser(string userIdToUnfollow)
		{
			var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

			if (currentUser != null)
			{
				// Kullanıcıyı takip etmeyi bırakma işlemi
				var follow = await _context.Follows
					.Where(f => f.FollowerId == currentUser.Id && f.FollowedUserId == userIdToUnfollow)
					.FirstOrDefaultAsync();

				if (follow != null)
				{
					_context.Follows.Remove(follow);
					await _context.SaveChangesAsync();
				}
			}

			// İşlem tamamlandığında kullanıcıya geri dön
			return RedirectToAction("AcademicianDetails", new { id = userIdToUnfollow });
		}

		public async Task<IActionResult> FollowerFollowing(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			var _currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

			var followersList = await _context.Follows
			   .Where(f => f.FollowedUserId == id)
			   .Select(f => f.FollowerId)
			   .ToListAsync();

			var followingsList = await _context.Follows
				.Where(f => f.FollowerId == id)
				.Select(f => f.FollowedUserId)
				.ToListAsync();

			var CurrentUserFollowList = await _context.Follows
					.Where(f => f.FollowerId == _currentUser.Id)
					.Join(_userManager.Users,
						follow => follow.FollowedUserId,
						user => user.Id,
						(follow, user) => user)
					.ToListAsync();



			var followers = await _userManager.Users
					.Where(u => followersList.Contains(u.Id))
					.ToListAsync();

			var following = await _userManager.Users
				.Where(u => followingsList.Contains(u.Id))
				.ToListAsync();



			var FollowModel = new FollowersFollowingModel
			{
				UserId = user,
				CurrentUser = _currentUser,
				Followers = followers,
				Following = following,
				UserFullName = user.FirstName + " " + user.LastName.ToUpper(),
				CurrentUserFollowList = CurrentUserFollowList
			};

			return View(FollowModel);
		}



		[HttpPost]

		public async Task<IActionResult> AddComment(int announcementID, string commentContent)
		{

			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			var announcement = _context.Announcements
				.Include(a => a.Comments) // Yorumları dahil et
				.FirstOrDefault(a => a.ID == announcementID);

			var SenderId = announcement.AnnouncementSenderID;

			var comment = new Comment
			{
				AnnouncementSenderId = SenderId,
				AnnouncementId = announcementID,
				Text = commentContent,
				UserId = user.Id,
				User = user,
				DatePosted = DateTime.Now,
				AnnouncementTitle = announcement.AnnouncementTitle,

			};

			_context.Comments.Add(comment);
			await _context.SaveChangesAsync();

			return RedirectToAction("AnnouncementDetails", new { ID = announcementID });
		}


		[HttpPost]
		[Authorize]
		public async Task<IActionResult> DeleteComment(int commentID)
		{
			var commentToDelete = await _context.Comments.FindAsync(commentID);

			if (commentToDelete == null)
			{
				return NotFound();
			}

			var user = await _userManager.GetUserAsync(User);

			if (commentToDelete.UserId != user.Id)
			{
				return Unauthorized();
			}

			_context.Comments.Remove(commentToDelete);
			await _context.SaveChangesAsync();

			return RedirectToAction("AnnouncementDetails", new { ID = commentToDelete.AnnouncementId });
		}

		[Authorize]
		public async Task<IActionResult> News()
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);

			// Kullanıcının takip ettiği kişilerin Id'lerini alın
			var followedUserIds = await _context.Follows
				.Where(f => f.FollowerId == user.Id)
				.Select(f => f.FollowedUserId)
				.ToListAsync();

			// Kullanıcının son giriş tarihini alın
			var lastLogin = user.LastLogin;

                // Kullanıcının takip ettiği kişilerin duyurularını çekin ve sıralayın
            var newAnnouncements = await _context.Announcements
				.Where(a => followedUserIds.Contains(a.AnnouncementSenderID) && a.AnnouncementSentDate > lastLogin)
				.OrderByDescending(a => a.AnnouncementSentDate)
				.ToListAsync();

			if(newAnnouncements.Count > 0)
			{
				newsPageAnnouncementsList.AddRange(newAnnouncements);
			}
			

			// Duyuruları bir model içinde taşıyın ve view'e gönderin
			var model = new NewAnnouncementsViewModel
			{
				FollowerAnnouncements = newsPageAnnouncementsList,
			};

			// Kullanıcının son giriş tarihini güncelleyin
			user.LastLogin = DateTime.Now;
			await _userManager.UpdateAsync(user);

			return View(model);
		}



	}

}

