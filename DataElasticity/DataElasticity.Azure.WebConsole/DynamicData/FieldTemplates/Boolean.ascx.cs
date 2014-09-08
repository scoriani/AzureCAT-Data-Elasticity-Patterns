#region usings

using System;
using System.Web.DynamicData;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class BooleanField : FieldTemplateUserControl
    {
        #region properties

        public override Control DataControl
        {
            get { return CheckBox1; }
        }

        #endregion

        #region methods

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);

            var val = FieldValue;
            if (val != null)
                CheckBox1.Checked = (bool) val;
        }

        #endregion
    }
}