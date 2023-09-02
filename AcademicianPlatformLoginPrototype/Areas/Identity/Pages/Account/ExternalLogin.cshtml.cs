// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using AcademicianPlatform.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;
using System.Xml.Linq;
using MessagePack;
using Microsoft.CodeAnalysis.Options;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;

namespace AcademicianPlatform.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly MicrosoftSignInManager<ApplicationUser> _microsoftSignInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IWebHostEnvironment _environment;        

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment,
			IHttpContextAccessor contextAccessor,
            MicrosoftSignInManager<ApplicationUser> microsoftSignInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _environment = webHostEnvironment;
            _microsoftSignInManager = microsoftSignInManager;
        }
        public bool isWhiteListed = false;
        private static ExternalLoginInfo info { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }
		private const string LoginProviderKey = "LoginProvider";
		private const string XsrfKey = "XsrfId";

		public string ReturnUrl { get; set; }
        public List<string> Departments = new List<string>
        {
            "",
            "Tıp",
            "Diş Hekimliği",
            "Eczacılık",
            "Bilgisayar Mühendisliği",
            "Endüstri Mühendisliği",
            "Endüstriyel Tasarım Mühendisliği",
            "Kimya Mühendisliği",
            "Makine Mühendisliği",
            "Mekatronik Mühendisliği",
            "Mimarlık",
            "Moleküler Biyoloji ve Genetik",
            "Yazılım Mühendisliği",
            "Beslenme ve Diyetetik",
            "Ergoterapi",
            "Fizyoterapi ve Rehabilitasyon",
            "Hemşirelik",
            "Odyoloji",
            "İngiliz Dili ve Edebiyatı",
            "İşletme",
            "Psikoloji",
            "İngilizce Mütercim-Tercümanlık",
            "İngilizce Hazırlık Birimi",
            "Ortak Yabancı Dil Dersleri Birimi",
            "Anestezi",
            "Ağız ve Diş Sağlığı",
            "İlk ve Acil Yardım",
            "Tıbbi Görüntüleme Teknikleri"
        };
        public List<string> Titles = new List<string>
        {
            "",
            "Prof. Dr.",
            "Doç. Dr.",
            "Yrd. Doç. Dr.",
            "Dr. Öğr. Üyesi",
            "Öğr. Üyesi",
            "Arş. Gör."
        };

        [TempData]
        public string ErrorMessage { get; set; }
        public List<string> WhitelistedEmails { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [Display(Name = "Telefon Numarası")]
            public string PhoneNumber { get; set; }
            [Required]
            [Display(Name = "İsim")]
            public string FirstName { get; set; }
            [Required]
            [Display(Name = "Soyisim")]
            public string LastName { get; set; }
            [Required]
            [Display(Name = "Hakkımda")]
            public string AboutMeText { get; set; }
            [Required]
            [Display(Name = "Departman")]
            public int Department { get; set; }
            [Required]
            [Display(Name = "Unvan")]
            public int Title { get; set; }
            public IFormFile CV { get; set; }
            [Display(Name = "Profil Resmi")]
            public IFormFile ProfilePhoto { get; set; }
        }
        public IActionResult OnGet() => RedirectToPage("./Login");

		public IActionResult OnPost(string provider, string returnUrl = null)
		{
			// Request a redirect to the external login provider.
			var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            info = await _microsoftSignInManager.GetMicrosoftExternalLoginInfoAsync(null,IdentityConstants.ExternalScheme);

            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            string emailFromMicrosoft = string.Empty;
            //Getting email information coming back from Microsoft API to check in the whitelist to see if it exists in the list. If it is, then user is an academician, they may proceed. If it isn't, then we reject.
            foreach (var claim in info.Principal.Claims)
            {
                if (claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                {
                    emailFromMicrosoft = claim.Value;
                }
            }
            //Let's read whitelist.json file.
            string whiteListContent = System.IO.File.ReadAllText(Path.Combine(_environment.ContentRootPath, "External", "whitelist.json"));
            var whiteList = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(whiteListContent);
            List<string> emailsInWhiteList = whiteList["emails"];
            foreach (var email in emailsInWhiteList)
            {
                if (email == emailFromMicrosoft)
                {
                    isWhiteListed = true;
                }
            }
            if (isWhiteListed == false)
            {
                return BadRequest("I'm sorry but you are not whitelisted!");
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
			// Get the information about the user from the external login provider
			// var info = await _microsoftSignInManager.GetMicrosoftExternalLoginInfoAsync(null,IdentityConstants.ExternalScheme);

			////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// OnGetCallbackAsync() function returns info variable successfully, however in OnPostConfirmationAsync() function returns null.                                  //
			// Since in both functions we are trying to access same data, I decided to keep the first "info" variable we retrieve from OnGetCallbackAsync() and keep it as a  //
			// static variable. We are not retrieving the same variable in OnPostConfirmationAsync() anymore because it returns null, we are using the old one.               //
            //                                                                                                                                                                //
			// I don't know if it's going to cause problems in the future, but it looks like it's working right now.                                                          //
			// If you encounter anymore issues related to this problem, try to find another way. Here are the steps to revert back to old version:                            //
			// 1- Uncomment "var info = await _microsoftSignInManager.GetMicrosoftExternalLoginInfoAsync(null,IdentityConstants.ExternalScheme);" on line 229.                //
			// 2- Delete the static "info" property of type ExternalLoginInfo.
            // 3- Put "var" in the beginning of line 166.
			////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                string userName = TruncateEmail(Input.Email);
                await _userStore.SetUserNameAsync(user, userName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.PhoneNumber = Input.PhoneNumber;
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.AboutMeText = Input.AboutMeText;
                user.Department = Departments[Input.Department];
                user.Title = Titles[Input.Title];

                string cvName = Input.CV.FileName;
                string cvPath = Path.Combine(_environment.WebRootPath, "cvs", cvName);
                string relativeCVPath = Path.GetRelativePath(_environment.WebRootPath, cvPath);
                using (var fileStream = new FileStream(cvPath, FileMode.Create))
                {
                    await Input.CV.CopyToAsync(fileStream);
                }
                user.CVPath = "/" + relativeCVPath;

                string profilePhotoName = Input.ProfilePhoto.FileName;
                string profilePhotoPath = Path.Combine(_environment.WebRootPath, "profilephotos", profilePhotoName);
                string relativeProfilePhotoPath = Path.GetRelativePath(_environment.WebRootPath, profilePhotoPath);
                using (var fileStream = new FileStream(profilePhotoPath, FileMode.Create))
                {
                    await Input.ProfilePhoto.CopyToAsync(fileStream);
                }
                user.ProfilePhotoPath = "/" + relativeProfilePhotoPath;


                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            
            ReturnUrl = returnUrl;
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }
        
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
        static string TruncateEmail(string email)
        {
            int atIndex = email.IndexOf('@');
            if (atIndex != -1)
            {
                return email.Substring(0, atIndex);
            }
            return email; // Return the original email if "@" is not found
        }
	}
}
