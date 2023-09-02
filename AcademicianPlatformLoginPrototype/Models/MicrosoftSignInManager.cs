using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Identity;
public class MicrosoftSignInManager<TUser> : SignInManager<TUser> where TUser : class
{
	private const string LoginProviderKey = "LoginProvider";
	private const string XsrfKey = "XsrfId";
	public MicrosoftSignInManager(
		UserManager<TUser> userManager,
		IHttpContextAccessor contextAccessor,
		IUserClaimsPrincipalFactory<TUser> claimsFactory,
		IOptions<IdentityOptions> optionsAccessor,
		ILogger<MicrosoftSignInManager<TUser>> logger,
		IAuthenticationSchemeProvider schemes,
		IUserConfirmation<TUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
	{
		if (userManager == null)
		{
			throw new ArgumentNullException(nameof(userManager));
		}
		if (contextAccessor == null)
		{
			throw new ArgumentNullException(nameof(contextAccessor));
		}
		if (claimsFactory == null)
		{
			throw new ArgumentNullException(nameof(claimsFactory));
		}

		UserManager = userManager;
		_contextAccessor = contextAccessor;
		ClaimsFactory = claimsFactory;
		Options = optionsAccessor?.Value ?? new IdentityOptions();
		Logger = logger;
		_schemes = schemes;
		_confirmation = confirmation;
	}

	private readonly IHttpContextAccessor _contextAccessor;
	private readonly IAuthenticationSchemeProvider _schemes;
	private readonly IUserConfirmation<TUser> _confirmation;
	public new UserManager<TUser> UserManager { get; set; }
	private HttpContext? _context;

	public new virtual ILogger Logger { get; set; }
	public new IUserClaimsPrincipalFactory<TUser> ClaimsFactory { get; set; }
	public new IdentityOptions Options { get; set; }
	public new HttpContext Context
	{
		get
		{
			var context = _context ?? _contextAccessor?.HttpContext;
			if (context == null)
			{
				throw new InvalidOperationException("HttpContext must not be null.");
			}
			return context;
		}
		set
		{
			_context = value;
		}
	}
	public virtual async Task<ExternalLoginInfo?> GetMicrosoftExternalLoginInfoAsync(string? expectedXsrf = null, string? scheme = null)
	{
		var auth = await Context.AuthenticateAsync(scheme);
		if (auth?.Principal == null)
		{
			return null;
		}
		var providerKey = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
		var provider = "Microsoft";
		if (providerKey == null || provider == null)
		{
			return null;
		}

		var providerDisplayName = (await GetExternalAuthenticationSchemesAsync()).FirstOrDefault(p => p.Name == provider)?.DisplayName
									?? provider;
		return new ExternalLoginInfo(auth.Principal, provider, providerKey, providerDisplayName)
		{
			AuthenticationTokens = auth.Properties?.GetTokens(),
			AuthenticationProperties = auth.Properties
		};
	}
}
