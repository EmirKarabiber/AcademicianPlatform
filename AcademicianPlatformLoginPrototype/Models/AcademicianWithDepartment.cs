using AcademicianPlatform.Areas.Identity.Data;

namespace AcademicianPlatform.Models
{
    public class AcademicianWithDepartment
    {
        public string GroupName { get; set; }
        public string Department { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }

}
