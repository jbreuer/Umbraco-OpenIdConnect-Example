using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco_OpenIdConnect_Example.Core.Extensions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco_OpenIdConnect_Example.Core.Member;

public class CustomMemberManager : MemberManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IMemberUserStore _store;
    private MemberIdentityUser? _currentMember;

    public CustomMemberManager(IIpResolver ipResolver, IMemberUserStore store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<MemberIdentityUser> passwordHasher, IEnumerable<IUserValidator<MemberIdentityUser>> userValidators, IEnumerable<IPasswordValidator<MemberIdentityUser>> passwordValidators, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<MemberIdentityUser>> logger, IOptionsSnapshot<MemberPasswordConfigurationSettings> passwordConfiguration, IPublicAccessService publicAccessService, IHttpContextAccessor httpContextAccessor) : base(ipResolver, store, optionsAccessor, passwordHasher, userValidators, passwordValidators, errors, services, logger, passwordConfiguration, publicAccessService, httpContextAccessor)
    {
    }

    public override Task<MemberIdentityUser?> GetUserAsync(ClaimsPrincipal principal)
    {
        var id = GetUserId(principal);
        
        // In the default implementation, the member is fetched from the database.
        // Since our member is from an external login provider we just build a virtual member.
        var user = this.CreateVirtualUser(id, principal.Claims);
        
        return Task.FromResult(user);
    }

    public override Task<IList<string>> GetRolesAsync(MemberIdentityUser user)
    {   
        // Multiple roles could be supported, but we don't need them in this example.
        var role = user.Claims.FirstOrDefault(x => x.ClaimType == ClaimTypes.Role)?.ClaimValue;
        var roles = new List<string>
        {
            role
        };
        
        return Task.FromResult((IList<string>)roles);
    }
}