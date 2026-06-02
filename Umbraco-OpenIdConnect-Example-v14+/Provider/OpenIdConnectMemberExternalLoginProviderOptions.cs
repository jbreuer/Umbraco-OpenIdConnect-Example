namespace Umbraco_OpenIdConnect_Example_v14plus.Provider;

using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Core;

public class OpenIdConnectMemberExternalLoginProviderOptions : IConfigureNamedOptions<MemberExternalLoginProviderOptions>
{
    public const string SchemeName = "OpenIdConnect";

    public void Configure(string? name, MemberExternalLoginProviderOptions options)
    {
        if (name != Constants.Security.MemberExternalAuthenticationTypePrefix + SchemeName)
        {
            return;
        }

        Configure(options);
    }

    public void Configure(MemberExternalLoginProviderOptions options)
    {
        options.AutoLinkOptions = new MemberExternalSignInAutoLinkOptions(
            autoLinkExternalAccount: true,
            defaultCulture: null,
            defaultIsApproved: true,
            defaultMemberTypeAlias: Constants.Security.DefaultMemberTypeAlias,
            defaultMemberGroups: new List<string> { "example-group" }
        )
        {
            ExternalOnly = true,
            OnAutoLinking = (autoLinkUser, loginInfo) =>
            {
                autoLinkUser.ProfileData = JsonSerializer.Serialize(BuildProfile(loginInfo), JsonSerializerOptions.Web);
            },
            OnExternalLogin = (user, loginInfo) =>
            {
                user.ProfileData = JsonSerializer.Serialize(BuildProfile(loginInfo), JsonSerializerOptions.Web);
                return true;
            }
        };
    }

    private static MemberProfile BuildProfile(ExternalLoginInfo loginInfo) => new()
    {
        Email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email),
        Name = loginInfo.Principal.FindFirstValue(ClaimTypes.Name),
    };

    private sealed class MemberProfile
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
}