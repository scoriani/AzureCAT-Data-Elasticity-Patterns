#region usings

using System;
using System.Web.DynamicData;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.EntityTemplates
{
    public partial class Default_EditEntityTemplate : EntityTemplateUserControl
    {
        #region fields

        private MetaColumn currentColumn;

        #endregion

        #region methods

        protected void DynamicControl_Init(object sender, EventArgs e)
        {
            var dynamicControl = (DynamicControl) sender;
            dynamicControl.DataField = currentColumn.Name;
        }

        protected void Label_Init(object sender, EventArgs e)
        {
            var label = (Label) sender;
            label.Text = currentColumn.DisplayName;
        }

        protected void Label_PreRender(object sender, EventArgs e)
        {
            var label = (Label) sender;
            var dynamicControl = (DynamicControl) label.FindControl("DynamicControl");
            var ftuc = dynamicControl.FieldTemplate as FieldTemplateUserControl;
            if (ftuc != null && ftuc.DataControl != null)
            {
                label.AssociatedControlID = ftuc.DataControl.GetUniqueIDRelativeTo(label);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            foreach (var column in Table.GetScaffoldColumns(Mode, ContainerType))
            {
                currentColumn = column;
                Control item = new DefaultEntityTemplate._NamingContainer();
                EntityTemplate1.ItemTemplate.InstantiateIn(item);
                EntityTemplate1.Controls.Add(item);
            }
        }

        #endregion
    }
}