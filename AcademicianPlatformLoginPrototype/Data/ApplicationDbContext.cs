using AcademicianPlatformLoginPrototype.Areas.Identity.Data;
using AcademicianPlatformLoginPrototype.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcademicianPlatformLoginPrototype.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		public DbSet<Announcement>? Announcements { get; set; }
	}
}