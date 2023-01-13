using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco_OpenIdConnect_Example.Core;

public class CustomMemberSignInManager : MemberSignInManager
{
    public CustomMemberSignInManager(UserManager<MemberIdentityUser> memberManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<MemberIdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<MemberIdentityUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<MemberIdentityUser> confirmation, IMemberExternalLoginProviders memberExternalLoginProviders, IEventAggregator eventAggregator) : base(memberManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation, memberExternalLoginProviders, eventAggregator)
    {
    }

    [Obsolete("Use ctor with all params")]
    public CustomMemberSignInManager(UserManager<MemberIdentityUser> memberManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<MemberIdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<MemberIdentityUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<MemberIdentityUser> confirmation) : base(memberManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }

    public override Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null)
    {
        return base.GetExternalLoginInfoAsync(expectedXsrf);
    }
}