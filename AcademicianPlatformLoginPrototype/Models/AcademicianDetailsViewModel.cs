using AcademicianPlatform.Areas.Identity.Data;

namespace AcademicianPlatform.Models
{
    public class AcademicianDetailsViewModel
    {
        public ApplicationUser Academian { get; set; }
        //public string UserId { get; set; }
        //public string UserName { get; set; }
        //public string Email { get; set; }
        //public string PhoneNumber { get; set; }
        //public string? ProfilePhotoPath { get; set; }
        
        public string FullName { get; set; }
        //public string? Department { get; set; }
        //public string? Title { get; set; }
        //public string? AboutMeText { get; set; }
        //public string? CVPath { get; set; }
        //public string? LastLogin { get; set; }
        // Diğer kullanıcı bilgileri eklemek için gerekli özellikleri ekleyin.
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        public bool? IsCurrentUser { get; set; }
        public bool? IsFollowing { get; set; }
        public List<Announcement> AcademianAnnouncements { get; set; }
    }
}
