namespace Umbraco_OpenIdConnect_Example.Core.Provider
{
    using System.Collections.Generic;
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

        // This method is based on the documentation:
        // https://our.umbraco.com/documentation/reference/security/auto-linking/#example-for-members
        public void Configure(MemberExternalLoginProviderOptions options)
        {
            options.AutoLinkOptions = new MemberExternalSignInAutoLinkOptions(
                // Must be true for auto-linking to be enabled
                autoLinkExternalAccount: true,

                // Optionally specify the default culture to create
                // the user as. If null it will use the default
                // culture defined in the web.config, or it can
                // be dynamically assigned in the OnAutoLinking
                // callback.
                defaultCulture: null,

                // Optionally specify the default "IsApprove" status. Must be true for auto-linking.
                defaultIsApproved: true,

                // Optionally specify the member type alias. Default is "Member"
                defaultMemberTypeAlias: "Member",

                // Optionally specify the member groups names to add the auto-linking user to.
                defaultMemberGroups: new List<string> { "example-group" }
            )
            {
                // Optional callback
                OnAutoLinking = (autoLinkUser, loginInfo) =>
                {
                    // You can customize the user before it's linked.
                    // i.e. Modify the user's groups based on the Claims returned
                    // in the externalLogin info
                },
                OnExternalLogin = (user, loginInfo) =>
                {   
                    // You can customize the user before it's saved whenever they have
                    // logged in with the external provider.
                    // i.e. Sync the user's name based on the Claims returned
                    // in the externalLogin info

                    return true; //returns a boolean indicating if sign in should continue or not.
                }
            };
        }
    }
}