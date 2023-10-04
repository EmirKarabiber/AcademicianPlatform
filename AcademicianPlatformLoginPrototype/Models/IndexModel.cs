using AcademicianPlatform.Areas.Identity.Data;

namespace AcademicianPlatform.Models
{
	public class IndexModelForNews
	{
        public ApplicationUser NewFollowerUsers { get; set; }
        public Follow NewFollowersFollow { get; set; }
    }
	public class IndexModel
	{
        public List<Announcement> AllAnnouncement { get; set; }
        public List<Announcement> SpecialAnnouncement { get; set; }
		public List<AnnouncementViewModel> AnnouncementViewModels { get; set; }
		public List<Announcement> announcements { get; set; }
        public int? IndexNews { get; set; }
		public List<Comment>? NewComments { get; set; }
		public List<IndexModelForNews>? NewFollowers { get; set; }
		public List<object> CombinedList { get; set; }

	}
	
}
