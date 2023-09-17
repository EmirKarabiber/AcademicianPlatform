using AcademicianPlatform.Areas.Identity.Data;
using MessagePack;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using System;
using System.Collections.Generic;

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
        
        public string? AnnouncementFaculty { get; set; }

        public bool? AnnouncementSpecial { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
