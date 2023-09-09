using AcademicianPlatform.Areas.Identity.Data;
using AcademicianPlatform.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcademicianPlatform.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		public DbSet<Announcement>? Announcements { get; set; }
        public DbSet<Follow> Follows { get; set; }
    }
}