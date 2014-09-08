#region usings

using System;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Servers
{
    public partial class Default : Page
    {
        #region methods

        protected void Page_Load(object sender, EventArgs e)
        {
            Title = "Servers";
            serverList.DataSource = Data.Elasticity.Models.Server.GetServers();
            serverList.DataBind();
        }

        #endregion
    }
}