using AcademicianPlatform.Areas.Identity.Data;

namespace AcademicianPlatform.Models
{
	public class Comment
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public string UserId { get; set; }
		public ApplicationUser User { get; set; }
		public bool IsDeleted { get; set; }
        public string AnnouncementSenderId { get; set; }
        public string AnnouncementTitle { get; set; }

        public DateTime DatePosted { get; set; }

		public int AnnouncementId { get; set; } // Yorumun hangi duyuruya ait olduğunu belirtmek için AnnouncementId ekleyin
		public Announcement Announcement { get; set; } // Duyuruya referans
	}
}
