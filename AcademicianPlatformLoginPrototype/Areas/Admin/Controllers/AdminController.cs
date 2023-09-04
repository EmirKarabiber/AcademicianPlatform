using AcademicianPlatform.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicianPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
    
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string deleteUserId)
        {
            var userToDelete = _context.Users.FirstOrDefault(p => p.Id == deleteUserId);
            if (userToDelete != null)
            {
                _context.Users.Remove(userToDelete);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
