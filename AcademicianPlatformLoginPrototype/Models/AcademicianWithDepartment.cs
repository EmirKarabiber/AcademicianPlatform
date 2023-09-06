using AcademicianPlatform.Areas.Identity.Data;

namespace AcademicianPlatform.Models
{
    public class AcademicianWithDepartment
    {
        public string Department { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }

}
