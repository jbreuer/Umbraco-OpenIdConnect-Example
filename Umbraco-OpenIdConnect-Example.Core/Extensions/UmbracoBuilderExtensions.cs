namespace Umbraco_OpenIdConnect_Example.Core.Extensions
{
    using System.Net;
    using System.Security.Claims;
    using Microsoft.Extensions.DependencyInjection;
    using Provider;
    using Umbraco.Cms.Core.DependencyInjection;
    using Umbraco.Extensions;

    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddOpenIdConnectAuthentication(this IUmbracoBuilder builder)
        {
            // Register GoogleMemberExternalLoginProviderOptions here rather than require it in startup
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
                                options.ResponseType = "code";
                                options.Scope.Add("openid");
                                options.Scope.Add("profile");
                                options.Scope.Add("email");
                                options.Scope.Add("phone");
                                options.Scope.Add("address");
                                options.RequireHttpsMetadata = true;
                                options.MetadataAddress = "https://dev-i92inbjg.us.auth0.com/.well-known/openid-configuration";
                                options.ClientId = "";
                                options.ClientSecret = "";
                                options.SaveTokens = true;
                                options.TokenValidationParameters.SaveSigninToken = true;
                                options.Events.OnTokenValidated = async context =>
                                {
                                    var claims = context?.Principal?.Claims.ToList();
                                    var email = claims?.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                                    if (email != null)
                                    {
                                        claims.Add(new Claim(ClaimTypes.Email, email.Value));
                                    }
                                    
                                    var name = claims?.SingleOrDefault(x => x.Type == "user_displayname");
                                    if (name != null)
                                    {
                                        claims.Add(new Claim(ClaimTypes.Name, name.Value));
                                    }

                                    if (context != null)
                                    {
                                        var authenticationType = context.Principal?.Identity?.AuthenticationType;
                                        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));
                                    }

                                    await Task.FromResult(0);
                                };
                                options.Events.OnRedirectToIdentityProviderForSignOut = async notification =>
                                {
                                    var protocolMessage = notification.ProtocolMessage;

                                    protocolMessage.IssuerAddress = "https://dev-i92inbjg.us.auth0.com/v2/logout?client_id=AOXaiUSRn6IH0aX7BKAFY7G7QIDI7HUx&returnTo=" + WebUtility.UrlEncode("https://localhost:44342/");
                                    await Task.FromResult(0);
                                };
                            });
                    });
            });
            return builder;
        }
    }    
}

