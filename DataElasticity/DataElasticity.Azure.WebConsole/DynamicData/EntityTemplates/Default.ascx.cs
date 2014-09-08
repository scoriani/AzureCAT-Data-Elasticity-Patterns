#region usings

using System;
using System.Web.DynamicData;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.EntityTemplates
{
    public partial class DefaultEntityTemplate : EntityTemplateUserControl
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

        protected override void OnLoad(EventArgs e)
        {
            foreach (var column in Table.GetScaffoldColumns(Mode, ContainerType))
            {
                currentColumn = column;
                Control item = new _NamingContainer();
                EntityTemplate1.ItemTemplate.InstantiateIn(item);
                EntityTemplate1.Controls.Add(item);
            }
        }

        #endregion

        #region nested type: _NamingContainer

        public class _NamingContainer : Control, INamingContainer
        {
        }

        #endregion
    }
}