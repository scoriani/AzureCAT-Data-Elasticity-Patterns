#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AzureCat.Patterns.Data.Elasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.WorkloadGroupConfigs
{
    public partial class Details : Page
    {
        #region fields

        protected TableGroupConfig currentItem = null;
        protected string lastMessage = "";

        #endregion

        #region methods

        public TableGroupConfig GetItem([QueryString] string id)
        {
            var item = TableGroupConfig.LoadCurrent(id);
            Title = item.TableGroupName + " Details";
            currentItem = item;
            return item;
        }

        public void InsertItem()
        {
            var item = new TableGroupConfig();
            if (ModelState.IsValid)
            {
                TryUpdateModel(item);
                item.Save();
                Response.Redirect("~/WorkloadGroupConfigs/default.aspx");
            }
        }

        // The id parameter name should match the DataKeyNames value set on the control
        public void UpdateItem(string WorkloadGroupName)
        {
            var item = TableGroupConfig.LoadCurrent(WorkloadGroupName);
            // Load the item here, e.g. item = MyDataLayer.Find(id);
            if (item == null)
            {
                // The item wasn't found
                ModelState.AddModelError("", String.Format("Item with id {0} was not found", WorkloadGroupName));
                return;
            }
            TryUpdateModel(item);
            if (ModelState.IsValid)
            {
                item.Save();
            }
        }

        // The id parameter should match the DataKeyNames value set on the control
        // or be decorated with a value provider attribute, e.g. [QueryString]int id

        public IList<Server> getServers()
        {
            return currentItem.Servers;
        }

        protected void ItemCommand(object sender, FormViewCommandEventArgs e)
        {
            if (e.CommandName.Equals("Cancel", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("Default");
            }
            else if (e.CommandName.Equals("Edit", StringComparison.OrdinalIgnoreCase))
            {
                dataForm.ChangeMode(FormViewMode.Edit);
            }
            else if (e.CommandName.Equals("CancelEdit", StringComparison.OrdinalIgnoreCase))
            {
                dataForm.ChangeMode(FormViewMode.ReadOnly);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["id"] != null)
            {
                dataForm.DefaultMode = FormViewMode.ReadOnly;
            }
            else
            {
                dataForm.DefaultMode = FormViewMode.Insert;
                currentItem = new TableGroupConfig();
                Title = "Create WorkloadGroup Configuration";
            }
        }

        protected void PointerShard_Command(object sender, CommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "AddShard":
                {
                    var data = HttpUtility.ParseQueryString(e.CommandArgument.ToString());
                    var workloadGroup = TableGroupConfig.LoadCurrent(data["WorkgroupConfigName"]);
                    var servername =
                        ((DropDownList) ((WebControl) sender).Parent.FindControl("newPointerShardList")).SelectedValue;
                    var catalog = ((TextBox) ((WebControl) sender).Parent.FindControl("newCatalogName")).Text;
                    var description = ((TextBox) ((WebControl) sender).Parent.FindControl("newDescription")).Text;
                    if (workloadGroup != null)
                    {
                        if (
                            workloadGroup.PointerShards.Count(
                                x => x.ServerInstanceName == servername && x.Catalog == catalog) == 0)
                        {
                            workloadGroup.PointerShards.Add(new PointerShard
                            {
                                Catalog = catalog,
                                Description = description,
                                ServerInstanceName = servername,
                            });
                            workloadGroup.Save();
                            dataForm.DataBind();
                        }
                    }
                    break;
                }
                case "UpdateShard":
                {
                    var data = HttpUtility.ParseQueryString(e.CommandArgument.ToString());
                    var workloadGroup = TableGroupConfig.LoadCurrent(data["WorkgroupConfigName"]);
                    var description = ((TextBox) ((WebControl) sender).Parent.FindControl("description")).Text;
                    int pointerShard;
                    if (workloadGroup != null)
                    {
                        if (int.TryParse(data["PointerShardID"], out pointerShard))
                        {
                            var shard = workloadGroup.PointerShards.FirstOrDefault(x => x.PointerShardID == pointerShard);
                            if (shard != null)
                            {
                                shard.Description = description;
                                workloadGroup.Save();
                                dataForm.DataBind();
                            }
                        }
                    }
                    break;
                }
                case "DeleteShard":
                {
                    var data = HttpUtility.ParseQueryString(e.CommandArgument.ToString());
                    var workloadGroup = TableGroupConfig.LoadCurrent(data["WorkgroupConfigName"]);
                    int pointerShard;
                    if (workloadGroup != null)
                    {
                        if (int.TryParse(data["PointerShardID"], out pointerShard))
                        {
                            var shard = workloadGroup.PointerShards.FirstOrDefault(x => x.PointerShardID == pointerShard);
                            if (shard != null)
                            {
                                workloadGroup.PointerShards.Remove(shard);
                                workloadGroup.Save();
                                dataForm.DataBind();
                            }
                        }
                    }
                    break;
                }
                case "DeployMap":
                {
                    var data = HttpUtility.ParseQueryString(e.CommandArgument.ToString());
                    var workloadGroup = TableGroupConfig.LoadCurrent(data["WorkgroupConfigName"]);
                    if (workloadGroup != null)
                    {
                        if (workloadGroup.PointerShards.Count > 0)
                        {
                            lastMessage = "Pointer Shards queued for deployment.";
                            workloadGroup.DeployPointerShards(true);
                        }
                        else
                        {
                            lastMessage = "No pointer shards to deploy.";
                        }
                    }
                    break;
                }
                case "MoveTenant":
                {
                    var data = HttpUtility.ParseQueryString(e.CommandArgument.ToString());
                    Response.Redirect(string.Format("~/WorkloadGroupConfigs/MoveTenant.aspx?id={0}",
                        data["WorkgroupConfigName"]));
                    break;
                }
            }
        }

        protected void ServerItem_Command(object sender, CommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "RemoveServer":
                {
                    var data = HttpUtility.ParseQueryString(e.CommandArgument.ToString());
                    var workloadGroup = TableGroupConfig.LoadCurrent(data["WorkgroupConfigName"]);
                    if (workloadGroup != null)
                    {
                        var servername = data["ServerInstanceName"];
                        var server = workloadGroup.Servers.FirstOrDefault(x => x.ServerInstanceName == servername);
                        if (server != null)
                        {
                            workloadGroup.Servers.Remove(server);
                            workloadGroup.Save();
                            currentItem = workloadGroup;
                            dataForm.DataBind();
                        }
                    }
                    break;
                }
                case "AddServer":
                {
                    var data = HttpUtility.ParseQueryString(e.CommandArgument.ToString());
                    var workloadGroup = TableGroupConfig.LoadCurrent(data["WorkgroupConfigName"]);
                    var servername =
                        ((DropDownList) ((WebControl) sender).Parent.FindControl("editServerList")).SelectedValue;
                    if (workloadGroup != null)
                    {
                        if (workloadGroup.Servers.Count(x => x.ServerInstanceName == servername) == 0)
                        {
                            var server = Data.Elasticity.Models.Server.Load(servername);
                            if (server != null)
                            {
                                workloadGroup.Servers.Add(server);
                                workloadGroup.Save();
                                currentItem = workloadGroup;
                                dataForm.DataBind();
                            }
                        }
                    }
                    break;
                }
            }
        }

        protected void Shard_Command(object sender, CommandEventArgs e)
        {
            var data = HttpUtility.ParseQueryString(e.CommandArgument.ToString());
            var workloadGroup = TableGroupConfig.LoadCurrent(data["WorkgroupConfigName"]);

            switch (e.CommandName)
            {
                case "UpdateMap":
                {
                    workloadGroup.UpdateShardMap();
                    workloadGroup.Save();
                    lastMessage = "Proposed Shard Map updated";
                    break;
                }
                case "DeployMap":
                {
                    workloadGroup.DeployShardMap(true);
                    lastMessage = "Shard Map deployment sent to the queue";
                    break;
                }
                case "PublishMap":
                {
                    workloadGroup.PublishShardMap(true);
                    lastMessage = "Shard Map publishing sent to the Queue";
                    break;
                }
            }
            dataForm.DataBind();
        }

        protected void dataForm_DataBound(object sender, EventArgs e)
        {
            var serverList = dataForm.FindControl("serversRepeater") as Repeater;
            if (serverList != null)
            {
                serverList.DataSource = currentItem.Servers;
                serverList.DataBind();
            }
            var publishedShardMap = dataForm.FindControl("publishedShardsRepeater") as Repeater;
            if (publishedShardMap != null)
            {
                if (currentItem.CurrentPublishedShardMapID != null)
                {
                    publishedShardMap.DataSource = ShardMap.Load(currentItem.CurrentPublishedShardMapID.Value).Shards;
                }
                else
                {
                    publishedShardMap.DataSource = new List<Shard>();
                }

                publishedShardMap.DataBind();
            }
            var proposedShardmap = dataForm.FindControl("proposedShardsRepeater") as Repeater;
            if (proposedShardmap != null)
            {
                if (currentItem.ShardMap != null)
                {
                    proposedShardmap.DataSource = currentItem.ShardMap.Shards;
                }
                proposedShardmap.DataBind();
            }
            var pointerShardList = dataForm.FindControl("pointerShardRepeater") as Repeater;
            if (pointerShardList != null)
            {
                pointerShardList.DataSource = currentItem.PointerShards;
                pointerShardList.DataBind();
            }
        }

        protected void proposedShardsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var repeater = (Repeater) sender;
            if (repeater.Items.Count < 1)
            {
                if (e.Item.ItemType == ListItemType.Footer)
                {
                    var lblFooter = (Literal) e.Item.FindControl("emptyMessage");

                    lblFooter.Visible = true;
                }
            }
            else
            {
                if (e.Item.ItemType == ListItemType.Footer)
                {
                    var lblFooter = (Literal) e.Item.FindControl("emptyMessage");

                    lblFooter.Visible = false;
                }
            }
        }

        protected void publishedShardsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var repeater = (Repeater) sender;
            if (repeater.Items.Count < 1)
            {
                if (e.Item.ItemType == ListItemType.Footer)
                {
                    var lblFooter = (Literal) e.Item.FindControl("emptyMessage");

                    lblFooter.Visible = true;
                }
            }
            else
            {
                if (e.Item.ItemType == ListItemType.Footer)
                {
                    var lblFooter = (Literal) e.Item.FindControl("emptyMessage");

                    lblFooter.Visible = false;
                }
            }
        }

        #endregion
    }
}