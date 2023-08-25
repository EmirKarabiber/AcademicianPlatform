using AcademicianPlatform.Areas.Identity.Data;
using MessagePack;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace AcademicianPlatform.Models
{
    public class Announcement
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int ID { get; set; }
        [Required]
        public string? AnnouncementTitle { get; set; }
        [Required]
        public string? AnnouncementContent { get; set; }
        [Required]
        public string AnnouncementSenderID { get; set; }
        [Required]
        public DateTime AnnouncementSentDate { get; set; }
    }
}
