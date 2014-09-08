#region usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AzureCat.Patterns.Data.Elasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Settings
{
    public partial class Default : Page
    {
        #region fields

        protected Server currentItem = null;

        #endregion

        // The id parameter should match the DataKeyNames value set on the control
        // or be decorated with a value provider attribute, e.g. [QueryString]int id

        #region methods

        public Data.Elasticity.Models.Settings GetItem()
        {
            var item = Data.Elasticity.Models.Settings.Load();
            return item;
        }

        public void UpdateItem(string ServerInstanceName)
        {
            var item = Data.Elasticity.Models.Settings.Load();
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
            Title = "Settings";
            dataForm.DefaultMode = FormViewMode.ReadOnly;
        }

        #endregion
    }
}