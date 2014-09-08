#region usings

using System;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Queues
{
    public partial class Default : Page
    {
        #region methods

        protected void Page_Load(object sender, EventArgs e)
        {
            Title = "Queues";
        }

        #endregion
    }
}