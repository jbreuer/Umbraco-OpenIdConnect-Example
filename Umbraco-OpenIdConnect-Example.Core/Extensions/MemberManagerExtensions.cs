using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Security;

namespace Umbraco_OpenIdConnect_Example.Core.Extensions;

public static class MemberManagerExtensions
{
    public static MemberIdentityUser CreateVirtualMember(this IMemberManager memberManager, string id, IEnumerable<Claim> claims)
    {
        var member = new MemberIdentityUser
        {
            Id = id,
            UserName = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value,
        };
        
        foreach(var claim in claims)
        {
            member.Claims.Add(new IdentityUserClaim<string>
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            });
        }
        member.IsApproved = true;
        
        var idToken = claims.FirstOrDefault(x => x.Type == "id_token")?.Value;
        if (!string.IsNullOrEmpty(idToken))
        {
            var loginIdToken = new IdentityUserToken(
                loginProvider: "UmbracoMembers.OpenIdConnect",
                name: "id_token",
                value: idToken,
                userId: null);
            member.LoginTokens.Add(loginIdToken);
        }

        return member;
    }
}