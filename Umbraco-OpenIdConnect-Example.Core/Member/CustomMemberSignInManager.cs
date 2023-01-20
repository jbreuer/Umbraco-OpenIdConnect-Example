using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco_OpenIdConnect_Example.Core.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco_OpenIdConnect_Example.Core.Member;

/// <summary>
///     The sign in manager for members
/// </summary>
public class CustomMemberSignInManager : UmbracoSignInManager<MemberIdentityUser>, IMemberSignInManager
{
    private readonly IMemberManager _memberManager;
    private readonly IEventAggregator _eventAggregator;
    private readonly IMemberExternalLoginProviders _memberExternalLoginProviders;

    public CustomMemberSignInManager(
        UserManager<MemberIdentityUser> memberManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<MemberIdentityUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<MemberIdentityUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<MemberIdentityUser> confirmation,
        IMemberExternalLoginProviders memberExternalLoginProviders,
        IEventAggregator eventAggregator)
        : base(memberManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _memberManager = (IMemberManager)memberManager;
        _memberExternalLoginProviders = memberExternalLoginProviders;
        _eventAggregator = eventAggregator;
    }

    [Obsolete("Use ctor with all params")]
    public CustomMemberSignInManager(
        UserManager<MemberIdentityUser> memberManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<MemberIdentityUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<MemberIdentityUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<MemberIdentityUser> confirmation)
        : this(
            memberManager,
            contextAccessor,
            claimsFactory,
            optionsAccessor,
            logger,
            schemes,
            confirmation,
            StaticServiceProvider.Instance.GetRequiredService<IMemberExternalLoginProviders>(),
            StaticServiceProvider.Instance.GetRequiredService<IEventAggregator>())
    {
    }

    // use default scheme for members
    protected override string AuthenticationType => IdentityConstants.ApplicationScheme;

    // use default scheme for members
    protected override string ExternalAuthenticationType => IdentityConstants.ExternalScheme;

    // use default scheme for members
    protected override string TwoFactorAuthenticationType => IdentityConstants.TwoFactorUserIdScheme;

    // use default scheme for members
    protected override string TwoFactorRememberMeAuthenticationType => IdentityConstants.TwoFactorRememberMeScheme;

    public override async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L422
        // to replace the auth scheme
        AuthenticateResult auth = await Context.AuthenticateAsync(ExternalAuthenticationType);
        IDictionary<string, string?>? items = auth.Properties?.Items;
        if (auth.Principal == null || items == null)
        {
            Logger.LogDebug(
                auth.Failure ??
                new NullReferenceException("Context.AuthenticateAsync(ExternalAuthenticationType) is null"),
                "The external login authentication failed. No user Principal or authentication items was resolved.");
            return null;
        }

        if (!items.ContainsKey(UmbracoSignInMgrLoginProviderKey))
        {
            throw new InvalidOperationException(
                $"The external login authenticated successfully but the key {UmbracoSignInMgrLoginProviderKey} was not found in the authentication properties. Ensure you call SignInManager.ConfigureExternalAuthenticationProperties before issuing a ChallengeResult.");
        }

        if (expectedXsrf != null)
        {
            if (!items.ContainsKey(UmbracoSignInMgrXsrfKey))
            {
                return null;
            }

            var userId = items[UmbracoSignInMgrXsrfKey];
            if (userId != expectedXsrf)
            {
                return null;
            }
        }

        var providerKey = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var provider = items[UmbracoSignInMgrLoginProviderKey];
        if (providerKey == null || provider is null)
        {
            return null;
        }

        var providerDisplayName =
            (await GetExternalAuthenticationSchemesAsync()).FirstOrDefault(p => p.Name == provider)?.DisplayName ??
            provider;
        return new ExternalLoginInfo(auth.Principal, provider, providerKey, providerDisplayName)
        {
            AuthenticationTokens = auth.Properties?.GetTokens(),
            AuthenticationProperties = auth.Properties,
        };
    }

    /// <summary>
    ///     Custom ExternalLoginSignInAsync overload for handling external sign in with auto-linking
    /// </summary>
    public async Task<SignInResult> ExternalLoginSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent, bool bypassTwoFactor = false)
    {
        // In the default implementation, the member is fetched from the database.
        // The default implementation also tries to create the member with the auto link feature if it doesn't exist.
        // The auto link feature has been removed here because the member is from an external login provider.
        // We just build a virtual member from the external login info.
        var claims = loginInfo.Principal.Claims;
        var id = claims.FirstOrDefault(x => x.Type == "sid")?.Value;
        var user = _memberManager.CreateVirtualUser(id, loginInfo.Principal.Claims);
        
        // For now hard code the role. These could be claims from the external login provider.
        user.Claims.Add(new IdentityUserClaim<string>() { ClaimType = ClaimTypes.Role, ClaimValue = "example-group" });

        return await SignInOrTwoFactorAsync(user, isPersistent, loginInfo.LoginProvider, bypassTwoFactor);
    }

    public override AuthenticationProperties ConfigureExternalAuthenticationProperties(
        string? provider,
        string? redirectUrl,
        string? userId = null)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
        // to be able to use our own XsrfKey/LoginProviderKey because the default is private :/
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        properties.Items[UmbracoSignInMgrLoginProviderKey] = provider;
        if (userId != null)
        {
            properties.Items[UmbracoSignInMgrXsrfKey] = userId;
        }

        return properties;
    }


    protected override async Task<SignInResult> SignInOrTwoFactorAsync(MemberIdentityUser user, bool isPersistent, string? loginProvider = null, bool bypassTwoFactor = false)
    {
        SignInResult result = await base.SignInOrTwoFactorAsync(user, isPersistent, loginProvider, bypassTwoFactor);

        if (result.RequiresTwoFactor)
        {
            NotifyRequiresTwoFactor(user);
        }

        return result;
    }

    protected void NotifyRequiresTwoFactor(MemberIdentityUser user) => Notify(
        user,
        currentUser => new MemberTwoFactorRequestedNotification(currentUser.Key));

    private T Notify<T>(MemberIdentityUser currentUser, Func<MemberIdentityUser, T> createNotification)
        where T : INotification
    {
        T notification = createNotification(currentUser);
        _eventAggregator.Publish(notification);
        return notification;
    }
}
