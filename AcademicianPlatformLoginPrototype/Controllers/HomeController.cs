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
		[Authorize]
		public async Task<IActionResult> Index()
		{
			// İki ay önceki tarihi hesaplayın
			var twoMonthsAgo = DateTime.Now.AddMonths(-1);

			// İki ay öncesinden sonraki duyuruları çekin ve tersten sıralayın.
			var announcements = _context.Announcements
				.Where(a => a.AnnouncementSentDate >= twoMonthsAgo)
				.OrderByDescending(a => a.ID)
				.ToList();

			// Kullanıcı girişi başarılı olduysa, kullanıcının son giriş tarihini güncelleyin
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user != null)
			{
				user.LastLogin = DateTime.Now; // Kullanıcının son giriş tarihini güncelle
				await _userManager.UpdateAsync(user); // Kullanıcıyı güncelle
			}

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
		public async Task<IActionResult> PostNewAnnouncement(string announcementTitle, string announcementContent, string senderName , string announcementFaculty, bool sendToAll)
		{
			var user = await _userManager.FindByNameAsync(senderName);
			Announcement announcement = new Announcement()
			{
				AnnouncementTitle = announcementTitle,
				AnnouncementContent = announcementContent,
				AnnouncementSentDate = DateTime.Now,
				AnnouncementSenderID = user.Id,
                AnnouncementFaculty = announcementFaculty
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


        //-------- bu kısım deneysel, mail eklerken herkese de gönderilsin mi gibi bir seçenek çıkmalı ve
        //-------- ona göre whitelistdeki herkese mail gönderecek
        public async Task <IActionResult> SendEmailAll(Announcement announcement)
        {
            // Retrieve the list of email addresses from your database
            var usersWithEmails = await _userManager.Users
                .Where(u => !string.IsNullOrEmpty(u.Email)) // email adresi girili hesaplara gönder
                .ToListAsync();

            var sentEmails = new List<string>();
            
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            foreach (var userWithEmail in usersWithEmails)
            {
                var recipientEmail = userWithEmail.Email;
                var message = new MimeMessage();
                var bodyBuilder = new BodyBuilder();
                message.From.Add(new MailboxAddress("Gönderen : " + user.Email, user.Email));
                message.To.Add(new MailboxAddress("Alıcı : " + recipientEmail, recipientEmail));
                message.Subject = announcement.AnnouncementTitle + " Hakkında";

            
                bodyBuilder.HtmlBody = "(Bu mail yeni bir duyuru eklendiğine dair bilgilendirmedir) " + announcement.AnnouncementContent;
                message.Body = bodyBuilder.ToMessageBody();

                // E-posta gönderme işlemi için SMTP istemcisini kullanma
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // SMTP sunucusuna bağlanma
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    // Kimlik doğrulama
                    await client.AuthenticateAsync("platformacademician@gmail.com", "eyiyoklvmbrnqfbw");
                    // E-postayı gönderme
                    await client.SendAsync(message);
                    // SMTP sunucusundan çıkma
                    await client.DisconnectAsync(true);
                }
                sentEmails.Add(recipientEmail);
            }

            var emailAddresses = sentEmails.ToArray(); // Convert the list to an array
            var recipientEmailsString = string.Join(", ", emailAddresses); // Join the email addresses with a comma and space

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

			// Mail bilgilerini hazırla
			string recipientEmail = "destek@example.com";
			string subject = "Konu";	//şu kısımlar bir şekilde sayfadan çekilecek
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



        [Authorize]
        public async Task<IActionResult> EmailSender([FromRoute(Name = "ID")] int announcementID)
        {
            // Mevcut kullanıcıyı bul
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

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
            
                    // E-postayı gönderme
                    await client.SendAsync(message);
            
                    // SMTP sunucusundan çıkma
                    await client.DisconnectAsync(true);
                }

                TempData["Message"] = "E-posta başarıyla gönderildi!";
                return RedirectToAction("EmailSenderResult",model);
            }

            // Model geçerli değilse formu tekrar göster
            return View("EmailSender", model);
        }

        public IActionResult EmailSenderResult(EmailViewModel model)
        {
            return View(model);
        }

		// Bu metot, belirli bir fakülte için duyuruları listeleyen bir sayfanın işlemesini sağlar.
		// İstenilen fakülte adı "announcementFaculty" parametresi ile alınır.
		public IActionResult IndexFaculty([FromQuery] string announcementFaculty)
		{
			// Eğer fakülte adı geçerli bir değere sahipse işlem yapılır.
			if (!string.IsNullOrEmpty(announcementFaculty))
			{
				// Veritabanından belirtilen fakültede yapılan duyuruları çeker.
				var announcements = _context.Announcements
					.Where(a => a.AnnouncementFaculty == announcementFaculty || a.AnnouncementFaculty == "Tüm Fakülteler")
					.OrderByDescending(a => a.ID)
					.ToList();

				// Duyuruları içeren bir görünümü döndürür.
				return View("Index", announcements);
			}

			// Eğer fakülte adı geçerli değilse, genel "Index" sayfasına yönlendirme yapılır.
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




        public IActionResult AcademicianDetails(string id)
        {
            var academician = _userManager.Users.FirstOrDefault(u => u.Id == id);
            var userAnnouncements = _context.Announcements
                .Where(a => a.AnnouncementSenderID == id)
                .OrderByDescending(a => a.ID)
                .ToList();
    
            var FullName = academician.FirstName + " " + academician.LastName.ToUpper();
            var viewModel = new AcademicianDetailsViewModel
            {
                UserId = academician.Id,
                UserName = academician.UserName,
                Email = academician.Email,
                PhoneNumber = academician.PhoneNumber,
                UserAnnouncements = userAnnouncements,
                ProfilePhotoPath = academician.ProfilePhotoPath,
                FullName = FullName,
                Department = academician.Department,
                Title = academician.Title,
                AboutMeText = academician.AboutMeText,
                CVPath = academician.CVPath,
				LastLogin = academician.LastLogin.ToString(),
				// Diğer kullanıcı bilgilerini burada doldurun.
			};

            return View(viewModel);
        }



    }
}