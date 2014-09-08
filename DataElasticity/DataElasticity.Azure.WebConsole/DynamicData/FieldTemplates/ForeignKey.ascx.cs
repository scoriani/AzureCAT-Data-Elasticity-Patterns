#region usings

using System;
using System.Web.DynamicData;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class ForeignKeyField : FieldTemplateUserControl
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

        protected string GetDisplayString()
        {
            var value = FieldValue;

            if (value == null)
            {
                return FormatFieldValue(ForeignKeyColumn.GetForeignKeyString(Row));
            }
            return FormatFieldValue(ForeignKeyColumn.ParentTable.GetDisplayString(value));
        }

        protected string GetNavigateUrl()
        {
            if (!AllowNavigation)
            {
                return null;
            }

            if (String.IsNullOrEmpty(NavigateUrl))
            {
                return ForeignKeyPath;
            }
            return BuildForeignKeyPath(NavigateUrl);
        }

        #endregion
    }
}