#region usings

using System;
using System.Web.UI;
using Microsoft.AzureCat.Patterns.Data.Elasticity.Util;
using Microsoft.AzureCat.Patterns.DataElasticity.SQLAzure;
using Microsoft.AzureCat.Patterns.DataElasticity.Testing.ApplicationData.TPCH;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Queries
{
    public partial class Default : Page
    {
        #region methods

        protected void ExecuteNonQuery_Click(object sender, EventArgs e)
        {
            var dal = new BaseTableGroupDAL();
            if (TenantToken.Text.Equals(""))
            {
                dal.ExecuteNonQuery(WorkloadGroupNames.Text, MyQuery.Text);
            }
            else
            {
                dal.ExecuteNonQuery(WorkloadGroupNames.Text, MyQuery.Text, TenantToken.Text.ToLower());
            }
        }

        protected void ExecuteQuery_Click(object sender, EventArgs e)
        {
            var dal = new BaseTableGroupDAL();

            if (TenantToken.Text.Equals(""))
            {
                QueryResults.DataSource = dal.ExecuteQuery(WorkloadGroupNames.Text, MyQuery.Text);
            }
            else
            {
                QueryResults.DataSource = dal.ExecuteQuery(WorkloadGroupNames.Text, MyQuery.Text,
                    TenantToken.Text.ToLower());
            }
            QueryResults.DataBind();
        }

        protected void GetDistributionKey_Click(object sender, EventArgs e)
        {
            TenantId.Text = CityHash.CityHash64StringGetLong(TenantToken.Text.ToLower()).ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "My new query window";
        }

        protected void SaveTenant_Click(object sender, EventArgs e)
        {
            var dal = new TpchDAL();

            //TODO: Replace this hard-code rudimentary demo code that demos DAL interaction
            dal.AddNewCustomer(TenantToken.Text, "DemoAddress", "CANADA", "DemoPhone", "DemoMarketSegment", "Demo Entry");
        }

        #endregion
    }
}