using AcademicianPlatform.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AcademicianPlatform.Data;
using AcademicianPlatform.Models;
using Microsoft.AspNetCore.Authorization;

namespace AcademicianPlatform.Areas.Identity.Pages.Account.Manage
{
    public class SupportModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public SupportModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [TempData]
        public string StatusMessage { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }
        public List<Ticket> Tickets { get; set; }
        public class InputModel
        {
            [Display(Name = "Kategori")]
            public string TicketType { get; set; }
            [Display(Name = "Baþlýk")]
            public string TicketTitle { get; set; }
            [Display(Name = "Ýçerik")]
            public string TicketContent { get; set; }
        }
        //public async void OnGet(string userName)
        //{
        //    var user = await _userManager.FindByNameAsync(userName);
        //    List<Ticket> allTickets = new List<Ticket>();
        //    allTickets = _context.Tickets.ToList();
        //    foreach(var ticket in allTickets)
        //    {
        //        if(ticket.TicketSenderId == user.Id)
        //        {
        //            Tickets.Add(ticket);
        //        }
        //    }
        //}
        public async Task<IActionResult> OnPostSubmitTicketAsync(string userName)
        {
            Ticket ticket = new()
            {
                TicketType = Input.TicketType,
                TicketTitle = Input.TicketTitle,
                TicketContent = Input.TicketContent,
                TicketDate = DateTime.Now,
                TicketSenderUserName = userName,
                TicketRespondContent = " ",
                TicketRespondSenderUserName = " ",
                isResolved = false
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            
            return RedirectToPage("Support");
        }
        public async Task<IActionResult> OnPostDeleteTicketAsync(int ticketId)
        {
            var ticketToDelete = await _context.Tickets.FindAsync(ticketId);

            if (ticketToDelete == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (ticketToDelete.TicketSenderUserName != user.UserName)
            {
                return Unauthorized();
            }

            _context.Tickets.Remove(ticketToDelete);
            await _context.SaveChangesAsync();

            return RedirectToPage("Support");
        }
    }
}
