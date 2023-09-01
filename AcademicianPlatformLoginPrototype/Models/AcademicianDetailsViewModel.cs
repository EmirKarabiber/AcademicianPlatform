namespace AcademicianPlatform.Models
{
    public class AcademicianDetailsViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        // Diğer kullanıcı bilgileri eklemek için gerekli özellikleri ekleyin.

        public List<Announcement> UserAnnouncements { get; set; }
    }
}
