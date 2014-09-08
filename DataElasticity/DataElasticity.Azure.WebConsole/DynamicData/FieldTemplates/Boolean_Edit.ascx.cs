#region usings

using System;
using System.Collections.Specialized;
using System.Web.DynamicData;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class Boolean_EditField : FieldTemplateUserControl
    {
        #region properties

        public override Control DataControl
        {
            get { return CheckBox1; }
        }

        #endregion

        #region methods

        protected override void ExtractValues(IOrderedDictionary dictionary)
        {
            dictionary[Column.Name] = CheckBox1.Checked;
        }

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