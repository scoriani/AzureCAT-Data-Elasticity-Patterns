#region usings

using System;
using System.Web;
using System.Web.Routing;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole
{
    public class Global : HttpApplication
    {
        #region methods

        private void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown
        }

        private void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }

        private void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        #endregion
    }
}