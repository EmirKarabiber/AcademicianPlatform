namespace AcademicianPlatform.Models
{
    public class Follow
    {
        public int Id { get; set; }
        public string FollowerId { get; set; } // Takip eden kullanıcının ID'si
        public string FollowedUserId { get; set; } // Takip edilen kullanıcının ID'si

        // Diğer özellikler veya ilişkiler
    }
}
