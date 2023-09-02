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
	public virtual async Task<ExternalLoginInfo?> GetMicrosoftExternalLoginInfoAsync(string? expectedXsrf = null)
	{
		var auth = await Context.AuthenticateAsync(IdentityConstants.ExternalScheme);
		//Debugging purposes
		Console.WriteLine(auth);
		//
		var items = auth?.Properties?.Items;
		//Debugging purposes
		foreach (var item in items)
		{
			Console.WriteLine(item);
		}
		//
		if (auth?.Principal == null || items == null || !items.ContainsKey(LoginProviderKey))
		{
			return null;
		}

		if (expectedXsrf != null)
		{
			if (!items.ContainsKey(XsrfKey))
			{
				return null;
			}
			var userId = items[XsrfKey] as string;
			if (userId != expectedXsrf)
			{
				return null;
			}
		}

		var providerKey = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
		//Debugging purposes
		Console.WriteLine(providerKey);
		//
		var provider = items[LoginProviderKey] as string;
		//Debugging purposes
		Console.WriteLine(provider);
		//
		if (providerKey == null || provider == null)
		{
			return null;
		}

		var providerDisplayName = (await GetExternalAuthenticationSchemesAsync()).FirstOrDefault(p => p.Name == provider)?.DisplayName
									?? provider;
		//Debugging purposes
		Console.WriteLine(providerDisplayName);
		//
		return new ExternalLoginInfo(auth.Principal, provider, providerKey, providerDisplayName)
		{
			AuthenticationTokens = auth.Properties?.GetTokens(),
			AuthenticationProperties = auth.Properties
		};
	}
}
