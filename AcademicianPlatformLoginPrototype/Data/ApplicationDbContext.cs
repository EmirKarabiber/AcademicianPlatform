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
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Ticket> Tickets { get; set; }
		public DbSet<MarqueeText> MarqueeText { get; set; }
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			// Yorumlar için duyuru ilişkisini tanımlayın
			builder.Entity<Comment>()
				.HasOne(c => c.Announcement)
				.WithMany(a => a.Comments)
				.HasForeignKey(c => c.AnnouncementId);
		}
	}
}