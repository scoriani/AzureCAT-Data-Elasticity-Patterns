#region usings

using System;
using System.Web.DynamicData;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class EmailAddressField : FieldTemplateUserControl
    {
        #region properties

        public override Control DataControl
        {
            get { return HyperLink1; }
        }

        #endregion

        #region methods

        protected override void OnDataBinding(EventArgs e)
        {
            var url = FieldValueString;
            if (!url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                url = "mailto:" + url;
            }
            HyperLink1.NavigateUrl = url;
        }

        #endregion
    }
}