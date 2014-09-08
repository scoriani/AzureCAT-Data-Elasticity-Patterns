#region usings

using System.Web.DynamicData;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class DateTimeField : FieldTemplateUserControl
    {
        #region properties

        public override Control DataControl
        {
            get { return Literal1; }
        }

        #endregion
    }
}