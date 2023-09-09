using AcademicianPlatform.Areas.Identity.Data;

namespace AcademicianPlatform.Models
{
    public class FollowersFollowingModel
    {
        public ApplicationUser? CurrentUser { get; set; }
        public List<ApplicationUser> Followers { get; set; }
        public List<ApplicationUser> Following { get; set; }
        public ApplicationUser? UserId { get; set; }
        public string? UserFullName { get; set; }
        public List<ApplicationUser> CurrentUserFollowList { get; set; }

    }
}
