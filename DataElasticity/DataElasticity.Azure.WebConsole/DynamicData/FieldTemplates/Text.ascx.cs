#region usings

using System.Web.DynamicData;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class TextField : FieldTemplateUserControl
    {
        #region constants

        private const int MAX_DISPLAYLENGTH_IN_LIST = 25;

        #endregion

        #region properties

        public override Control DataControl
        {
            get { return Literal1; }
        }

        public override string FieldValueString
        {
            get
            {
                var value = base.FieldValueString;
                if (ContainerType == ContainerType.List)
                {
                    if (value != null && value.Length > MAX_DISPLAYLENGTH_IN_LIST)
                    {
                        value = value.Substring(0, MAX_DISPLAYLENGTH_IN_LIST - 3) + "...";
                    }
                }
                return value;
            }
        }

        #endregion
    }
}