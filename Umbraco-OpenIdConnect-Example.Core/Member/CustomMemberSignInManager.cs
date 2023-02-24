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
public class CustomMemberSignInManager : MemberSignInManager
{
    private readonly IMemberManager _memberManager;

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
    
    public override async Task<SignInResult> ExternalLoginSignInAsync(
        ExternalLoginInfo loginInfo, 
        bool isPersistent, 
        bool bypassTwoFactor = false)
    {
        // In the default implementation, the member is fetched from the database.
        // The default implementation also tries to create the member with the auto link feature if it doesn't exist.
        // The auto link feature has been removed here because the member is from an external login provider.
        // We just build a virtual member from the external login info.
        var claims = loginInfo.Principal.Claims;
        var id = claims.FirstOrDefault(x => x.Type == "sid")?.Value;
        var member = _memberManager.CreateVirtualMember(id, loginInfo.Principal.Claims);
        
        // For now hard code the role. These could be claims from the external login provider.
        member.Claims.Add(new IdentityUserClaim<string>() { ClaimType = ClaimTypes.Role, ClaimValue = "example-group" });

        return await SignInOrTwoFactorAsync(member, isPersistent, loginInfo.LoginProvider, bypassTwoFactor);
    }
}