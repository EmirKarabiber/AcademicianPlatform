namespace AcademicianPlatform.Models
{
	public class IndexModel
	{
        public List<Announcement> AllAnnouncement { get; set; }
        public List<Announcement> SpecialAnnouncement { get; set; }
		public List<AnnouncementViewModel> AnnouncementViewModels { get; set; }
		public List<Announcement> announcements { get; set; }
	}
}
