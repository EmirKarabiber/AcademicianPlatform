namespace AcademicianPlatform.Models
{
    public class AcademicianDetailsViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public string FullName { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public string? AboutMeText { get; set; }
        public string? CVPath { get; set; }
        // Diğer kullanıcı bilgileri eklemek için gerekli özellikleri ekleyin.

        public List<Announcement> UserAnnouncements { get; set; }
    }
}
