using System.ComponentModel.DataAnnotations;

namespace AcademicianPlatform.Models
{
    public class EmailViewModel
    {
        [Required]
        public string SenderEmail { get; set; }
        [Required]
        public string RecipientEmail { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }
        [Required]
        public int AnnouncementIDForEmail { get; set; }
    }
}
