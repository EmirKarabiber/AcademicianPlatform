using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace AcademicianPlatform.Areas.Identity.Data
{
	public class ApplicationUser : IdentityUser
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? Department { get; set; }
		public string? Title { get; set; }
		public string? AboutMeText { get; set; }
		public string? ProfilePhotoPath { get; set; }
		public string? CVPath { get; set; }
		[NotMapped]
		public override bool EmailConfirmed { get; set; }
		[NotMapped]
		public override bool PhoneNumberConfirmed { get; set; }
		[NotMapped]
		public override bool TwoFactorEnabled { get; set; }
		[NotMapped]
		public override bool LockoutEnabled { get; set; }
		[NotMapped]
		public override DateTimeOffset? LockoutEnd { get; set; }
		[NotMapped]
		public override int AccessFailedCount { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
