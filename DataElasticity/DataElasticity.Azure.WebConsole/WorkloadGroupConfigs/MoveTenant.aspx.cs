#region usings

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AzureCat.Patterns.Data.Elasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.WorkloadGroupConfigs
{
    public partial class MoveTenant : Page
    {
        #region properties

        protected string lastMessage { get; set; }

        private string _workloadGroupName
        {
            get { return Request.QueryString["id"]; }
        }

        #endregion

        #region methods

        protected void Page_Load(object sender, EventArgs e)
        {
            if (_workloadGroupName != null)
            {
                if (!HasPointerShards())
                {
                    Response.Redirect("~/WorkloadGroupConfigs/default.aspx");
                }
            }
            else
            {
                Response.Redirect("~/WorkloadGroupConfigs/default.aspx");
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("~/WorkloadGroupConfigs/details.aspx?id={0}", _workloadGroupName));
        }

        protected void btnQueue_Click(object sender, EventArgs e)
        {
            long tenantID;
            string tenantIdTxt = null;

            //Check for a valid tenantID
            if (string.IsNullOrEmpty(txtTenantId.Text))
            {
                if (string.IsNullOrEmpty(txtToken.Text))
                {
                    lastMessage = "A Tenant Token or Tenant ID is required.";
                    return;
                }
                var token = txtToken.Text;
                tenantIdTxt = Tenant.GetDistributionKeyForToken(token).ToString();
            }
            else
            {
                tenantIdTxt = txtTenantId.Text;
            }

            //Parse the tenant ID
            if (!long.TryParse(txtTenantId.Text, out tenantID))
            {
                lastMessage = "A Tenant must be a 64bit integer.";
                return;
            }

            //Load the tenant
            var tenant = Tenant.Load(_workloadGroupName, tenantID);
            if (tenant.ServerInstanceName == null)
            {
                //Opps we didn't find one
                lastMessage = "Tenant not found";
                return;
            }

            //load the selected pointer shard
            var shardID = int.Parse(ddlNewShard.SelectedValue);
            var pointerShard = PointerShard.GetPointerShard(shardID);
            if (pointerShard == null)
            {
                lastMessage = "Pointer Shard not found";
                return;
            }

            //Check to see if the tenant is already in that shard
            if (tenant.Catalog == pointerShard.Catalog && tenant.ServerInstanceName == pointerShard.ServerInstanceName)
            {
                lastMessage = "Tenant was already in Shard. It has been pinned to make this relationship permanent";
                tenant.PinToCurrentShard();
            }
            tenant.MoveTo(pointerShard, true, true, Guid.NewGuid(), true);
            lastMessage = "Tenant move has been queued.";
        }

        protected void convertToDistributionKey_Click(object sender, EventArgs e)
        {
            var token = txtToken.Text;
            txtTenantId.Text = Tenant.GetDistributionKeyForToken(token).ToString();
        }

        private bool HasPointerShards()
        {
            var workloadGroup = TableGroupConfig.LoadCurrent(_workloadGroupName);
            if (workloadGroup.PointerShards.Count() > 0)
            {
                ddlNewShard.Items.Clear();
                foreach (var pointerShard in workloadGroup.PointerShards)
                {
                    ddlNewShard.Items.Add(new ListItem
                    {
                        Text = string.Format("{0} - {1}", pointerShard.ServerInstanceName, pointerShard.Catalog),
                        Value = pointerShard.PointerShardID.ToString(),
                    });
                }
                return true;
            }
            return false;
        }

        #endregion
    }
}