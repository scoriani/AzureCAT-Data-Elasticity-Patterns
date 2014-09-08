#region usings

using System;
using System.Web.UI;
using Microsoft.AzureCat.Patterns.Data.Elasticity;
using Microsoft.AzureCat.Patterns.Data.Elasticity.Models;
using Microsoft.AzureCat.Patterns.Data.Elasticity.Models.QueueMessages;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Queues
{
    public partial class PublishMapQueue : Page
    {
        #region methods

        protected void Filter_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("~/Queues/PublishMapQueue.aspx?Status={1}", Request.QueryString["id"],
                Filter.SelectedValue));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Title = "Publish Map Queue";
            if (IsPostBack) return;
            
            if (Request.QueryString["Status"] != null)
            {
                for (var i = 0; i < Filter.Items.Count; i++)
                {
                    if (Filter.Items[i].Value == Request.QueryString["Status"])
                    {
                        Filter.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                Filter.SelectedIndex = 0;
            }

            LoadList();
        }

        private void LoadList()
        {
            var filter = TableActionQueueItemStatus.Queued;
            try
            {
                filter =
                    (TableActionQueueItemStatus)
                        Enum.Parse(typeof (TableActionQueueItemStatus), Filter.SelectedValue);
            }
            catch
            {
                Response.Redirect("~/Queues/default.aspx");
            }

            queueList.DataSource = TableGroupActionQueue.GetQueuedRequestsByStatus<ShardMapPublishingRequest>(filter);
            queueList.DataBind();
        }

        #endregion
    }
}