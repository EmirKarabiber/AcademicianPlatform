using AcademicianPlatform.Areas.Identity.Data;

namespace AcademicianPlatform.Models
{
	public class FollowModelForIndexModel
	{
        public ApplicationUser NewFollowerUsers { get; set; }   //Notification sisteminden User a gidebilmek için
        public string followerid { get; set; }
        public Follow NewFollowersFollow { get; set; }		//followu belirtmek için
    }
	public class IndexModel
	{
        public List<Announcement> AllAnnouncement { get; set; }		//tüm duyurular
        public List<Announcement> SpecialAnnouncement { get; set; }		//özel duyurular
        public int? IndexNews { get; set; }		//New Announcement sayısını tutacak.
		public List<object> NotificationList { get; set; }

	}
	
}
