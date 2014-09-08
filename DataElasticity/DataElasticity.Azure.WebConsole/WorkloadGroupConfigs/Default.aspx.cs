#region usings

using System;
using System.Collections.Generic;
using System.Web.UI;
using Microsoft.AzureCat.Patterns.Data.Elasticity;
using Microsoft.AzureCat.Patterns.Data.Elasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.WorkloadGroupConfigs
{
    public partial class Default : Page
    {
        #region methods

        protected void Page_Load(object sender, EventArgs e)
        {
            Title = "Configs";
            workgroupList.DataSource = TableGroupConfig.GetTableList();
            workgroupList.DataBind();
        }

        #endregion
    }
}