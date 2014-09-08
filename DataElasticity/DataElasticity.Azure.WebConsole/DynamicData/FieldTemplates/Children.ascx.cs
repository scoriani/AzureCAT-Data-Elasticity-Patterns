#region usings

using System;
using System.Web.DynamicData;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class ChildrenField : FieldTemplateUserControl
    {
        #region fields

        private bool _allowNavigation = true;

        #endregion

        #region properties

        public bool AllowNavigation
        {
            get { return _allowNavigation; }
            set { _allowNavigation = value; }
        }

        public override Control DataControl
        {
            get { return HyperLink1; }
        }

        public string NavigateUrl { get; set; }

        #endregion

        #region methods

        protected string GetChildrenPath()
        {
            if (!AllowNavigation)
            {
                return null;
            }

            if (String.IsNullOrEmpty(NavigateUrl))
            {
                return ChildrenPath;
            }
            return BuildChildrenPath(NavigateUrl);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            HyperLink1.Text = "View " + ChildrenColumn.ChildTable.DisplayName;
        }

        #endregion
    }
}