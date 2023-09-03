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
            var jsonPath = "./External/whitelist.json"; // JSON dosyasının yolu
            var jsonData = System.IO.File.ReadAllText(jsonPath);
            var json = JObject.Parse(jsonData);

            var emailList = json["emails"].ToObject<string[]>();
            // emailList içeriğini kullanarak istediğiniz işlemleri gerçekleştirin

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            foreach (var recipientEmail in emailList)
            {
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
            }
            var emailModel = new EmailViewModel
            {
                RecipientEmail = emailList.ToString(),
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

        [Authorize]
        public IActionResult Academians()
        {
            // Tüm kullanıcıları çekin
            var allUsers = _userManager.Users.ToList();

            // Kullanıcıları departmanlarına göre gruplayın ve fakültelere göre eşleyin
            var groupedUsers = new List<AcademicianWithDepartment>();

            // Departmanları fakültelere eşleyen bir harita kullanın
            var departmentToFacultyMapping = DepartmentToFacultyMapping.MapDepartmentsToFaculties();

            foreach (var user in allUsers)
            {
                // Kullanıcının departmanını alın
                var department = user.Department;

                // Eşlenen fakülteyi bulun veya "Diğer Fakülteler" olarak kabul edin
                var faculty = departmentToFacultyMapping.ContainsKey(department) ? departmentToFacultyMapping[department] : "Diğer Fakülteler";

                // İlgili fakülteyi gruplama listesinde arayın veya oluşturun
                var group = groupedUsers.FirstOrDefault(g => g.Department == faculty);
                if (group == null)
                {
                    group = new AcademicianWithDepartment
                    {
                        Department = faculty,
                        Users = new List<ApplicationUser>()
                    };
                    groupedUsers.Add(group);
                }

                // Kullanıcıyı ilgili fakülteye ekleyin
                group.Users.Add(user);
            }

            // Her bir fakülte altındaki kullanıcıları alfabetik olarak sıralayın
            foreach (var group in groupedUsers)
            {
                group.Users = group.Users.OrderBy(u => u.Department).ToList();
            }

            // Fakülteleri alfabetik olarak sıralayın
            var sortedGroups = groupedUsers.OrderBy(group => group.Department).ToList();

            return View(sortedGroups);
        }



        public IActionResult AcademicianDetails(string id)
        {
            var academician = _userManager.Users.FirstOrDefault(u => u.Id == id);
            var userAnnouncements = _context.Announcements
                .Where(a => a.AnnouncementSenderID == id)
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
                // Diğer kullanıcı bilgilerini burada doldurun.
            };

            return View(viewModel);
        }



    }
}