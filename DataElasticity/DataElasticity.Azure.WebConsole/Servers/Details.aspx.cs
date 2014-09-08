using System;
using System.Web.ModelBinding;
using System.Web.UI.WebControls;
using Microsoft.AzureCat.Patterns.Data.Elasticity.Models;

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Servers
{
    public partial class Details : System.Web.UI.Page
    {
        protected Server currentItem = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["id"] != null)
            {
                dataForm.DefaultMode = FormViewMode.ReadOnly;
            }
            else
            {
                dataForm.DefaultMode = FormViewMode.Insert;
                currentItem = new Server();
                Title = "Create Server";
            }
        }
        public void InsertItem()
        {

            var item = new Server();
            if (ModelState.IsValid)
            {
                TryUpdateModel(item);
                item.Save();

                Response.Redirect("~/Servers/default.aspx");
            }
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

        // The id parameter name should match the DataKeyNames value set on the control
        public void UpdateItem(string ServerInstanceName)
        {
            Server item = Data.Elasticity.Models.Server.Load(ServerInstanceName);
            // Load the item here, e.g. item = MyDataLayer.Find(id);
            if (item == null)
            {
                // The item wasn't found
                ModelState.AddModelError("", String.Format("Item with id {0} was not found", ServerInstanceName));
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
        public Server GetItem([QueryString]string id)
        {
            var item = Data.Elasticity.Models.Server.Load(id);
            Title = item.ServerInstanceName + " Details";
            currentItem = item;
            return item;
        }
    }
}