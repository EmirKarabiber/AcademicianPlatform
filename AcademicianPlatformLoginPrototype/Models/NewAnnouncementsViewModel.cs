namespace AcademicianPlatform.Models
{
    public class NewAnnouncementsViewModel
    {
        public List<Announcement> FollowerAnnouncements { get; set; }
        public List<Comment>? NewComments { get; set; } // Yeni yorumları taşıyacak özellik
    }


}
