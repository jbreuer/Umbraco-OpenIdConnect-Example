namespace Umbraco_OpenIdConnect_Example.Core.Extensions
{
    using System.Net;
    using System.Security.Claims;
    using Microsoft.Extensions.DependencyInjection;
    using Provider;
    using Umbraco.Cms.Core.DependencyInjection;
    using Umbraco.Cms.Core.Security;
    using Umbraco.Extensions;

    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddOpenIdConnectAuthentication(this IUmbracoBuilder builder)
        {
            builder.Services.ConfigureOptions<OpenIdConnectMemberExternalLoginProviderOptions>();

            builder.AddMemberExternalLogins(logins =>
            {
                logins.AddMemberLogin(
                    memberAuthenticationBuilder =>
                    {
                        memberAuthenticationBuilder.AddOpenIdConnect(
                            // The scheme must be set with this method to work for the umbraco members
                            memberAuthenticationBuilder.SchemeForMembers(OpenIdConnectMemberExternalLoginProviderOptions.SchemeName),
                            options =>
                            {
                                var config = builder.Config;
                                options.ResponseType = "code";
                                options.Scope.Add("openid");
                                options.Scope.Add("profile");
                                options.Scope.Add("email");
                                options.Scope.Add("phone");
                                options.Scope.Add("address");
                                options.RequireHttpsMetadata = true;
                                options.MetadataAddress = config["OpenIdConnect:MetadataAddress"];
                                options.ClientId = config["OpenIdConnect:ClientId"];
                                // Normally the ClientSecret should not be in the Github repo.
                                // These settings are valid and only used for this example.
                                // So it's ok these are public.
                                options.ClientSecret = config["OpenIdConnect:ClientSecret"];
                                options.SaveTokens = true;
                                options.TokenValidationParameters.SaveSigninToken = true;
                                options.Events.OnTokenValidated = async context =>
                                {
                                    var claims = context?.Principal?.Claims.ToList();
                                    var email = claims?.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                                    if (email != null)
                                    {
                                        // The email claim is required for auto linking.
                                        // So get it from another claim and put it in the email claim.
                                        claims?.Add(new Claim(ClaimTypes.Email, email.Value));
                                    }

                                    var name = claims?.SingleOrDefault(x => x.Type == "user_displayname");
                                    if (name != null)
                                    {
                                        // The name claim is required for auto linking.
                                        // So get it from another claim and put it in the name claim.
                                        claims?.Add(new Claim(ClaimTypes.Name, name.Value));
                                    }

                                    if (context != null)
                                    {
                                        // Since we added new claims create a new principal.
                                        var authenticationType = context.Principal?.Identity?.AuthenticationType;
                                        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));
                                    }

                                    await Task.FromResult(0);
                                };
                                options.Events.OnRedirectToIdentityProviderForSignOut = async notification =>
                                {
                                    var protocolMessage = notification.ProtocolMessage;

                                    var logoutUrl = config["OpenIdConnect:LogoutUrl"];
                                    var returnAfterLogout = config["OpenIdConnect:ReturnAfterLogout"];
                                    if (!string.IsNullOrEmpty(logoutUrl) && !string.IsNullOrEmpty(returnAfterLogout))
                                    {
                                        protocolMessage.IssuerAddress =
                                            $"{config["OpenIdConnect:LogoutUrl"]}" +
                                            $"?client_id={config["OpenIdConnect:ClientId"]}" +
                                            $"&returnTo={WebUtility.UrlEncode(config["OpenIdConnect:ReturnAfterLogout"])}";
                                    }

                                    var memberManager = notification.HttpContext.RequestServices.GetService<IMemberManager>();
                                    if (memberManager != null)
                                    {
                                        var currentMember = await memberManager.GetCurrentMemberAsync();
                                        var idToken = currentMember?.LoginTokens?.FirstOrDefault(x => x.Name == "id_token");
                                        if (idToken != null && !string.IsNullOrEmpty(idToken.Value))
                                        {
                                            protocolMessage.IdTokenHint = idToken.Value;
                                        }
                                    }

                                    await Task.FromResult(0);
                                };
                            });
                    });
            });
            return builder;
        }
    }  
}

