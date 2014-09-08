using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            var settings = new FriendlyUrlSettings();
            settings.AutoRedirectMode = RedirectMode.Permanent;
            routes.EnableFriendlyUrls(settings);
        }
    }
}
