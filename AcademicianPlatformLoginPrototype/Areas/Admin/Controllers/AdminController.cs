using AcademicianPlatform.Data;
using Microsoft.AspNetCore.Mvc;

namespace AcademicianPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
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
    }
}
