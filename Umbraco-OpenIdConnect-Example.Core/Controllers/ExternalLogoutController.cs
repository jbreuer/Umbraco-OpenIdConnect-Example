namespace Umbraco_OpenIdConnect_Example.Core.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Umbraco.Cms.Core.Cache;
    using Umbraco.Cms.Core.Logging;
    using Umbraco.Cms.Core.Routing;
    using Umbraco.Cms.Core.Services;
    using Umbraco.Cms.Core.Web;
    using Umbraco.Cms.Infrastructure.Persistence;
    using Umbraco.Cms.Web.Common.Filters;
    using Umbraco.Cms.Web.Common.Security;
    using Umbraco.Cms.Web.Website.Controllers;
    using Microsoft.AspNetCore.Authentication;
    using Umbraco.Cms.Web.Common.Models;

    public class ExternalLogoutController : SurfaceController
    {
        private readonly IMemberSignInManager _signInManager;

        public ExternalLogoutController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberSignInManager signInManager)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _signInManager = signInManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> HandleLogout([Bind(Prefix = "logoutModel")]PostRedirectModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            var isLoggedIn = HttpContext.User?.Identity?.IsAuthenticated ?? false;

            if (isLoggedIn)
            {   
                await this.HttpContext.SignOutAsync("UmbracoMembers.OpenIdConnect");
                await _signInManager.SignOutAsync();
            }
            
            return new EmptyResult();
        }
    }
}