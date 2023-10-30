using AcademicianPlatform.Areas.Identity.Data;

namespace AcademicianPlatform.Models
{
	public class NewAnnouncementsViewModel   //For Follower Announcements Details Page
	{
		public List<Announcement> FollowerAnnouncements { get; set; }
	}

	public class AnnouncementViewModel		//For Announcement Details Page
	{
		public Announcement Announcement { get; set; }
		public List<Comment> Comments { get; set; }
		
		//public int Id { get; set; }
		//public string Text { get; set; }
		//public string UserId { get; set; }
		//public ApplicationUser User { get; set; }
		//public DateTime DatePosted { get; set; }
		//public bool IsDeleted { get; set; }

		//[System.ComponentModel.DataAnnotations.Key]
		
		//public int ID { get; set; }
		//public string AnnouncementTitle { get; set; }
		//public string AnnouncementContent { get; set; }
		//public string AnnouncementSenderID { get; set; }
		//public DateTime AnnouncementSentDate { get; set; }
		//public string AnnouncementFaculty { get; set; }
		//public bool AnnouncementSpecial { get; set; }
		//public int AnnouncementId { get; set; }
		
		/*
		// AnnouncementViewModel modelini Announcement modelinden dönüştürmek için bir dönüştürücü metodu ekleyin
		public static AnnouncementViewModel FromAnnouncement(Announcement announcement)
		{
			return new AnnouncementViewModel
			{
				Announcement = announcement,
				Comments = announcement.Comments,
				//Text = announcement.AnnouncementContent,
				//AnnouncementTitle = announcement.AnnouncementTitle,
				//AnnouncementContent = announcement.AnnouncementContent,
				//AnnouncementSenderID = announcement.AnnouncementSenderID,
				//AnnouncementSentDate = announcement.AnnouncementSentDate,
				//AnnouncementFaculty = announcement.AnnouncementFaculty,
				//AnnouncementSpecial = announcement.AnnouncementSpecial ?? false,
				
			};
		}
		*/
	}
	
}
