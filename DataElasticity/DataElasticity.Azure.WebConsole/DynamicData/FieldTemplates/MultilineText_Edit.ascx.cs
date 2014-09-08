#region usings

using System;
using System.Collections.Specialized;
using System.Web.DynamicData;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class MultilineText_EditField : FieldTemplateUserControl
    {
        #region properties

        public override Control DataControl
        {
            get { return TextBox1; }
        }

        #endregion

        #region methods

        protected override void ExtractValues(IOrderedDictionary dictionary)
        {
            dictionary[Column.Name] = ConvertEditedValue(TextBox1.Text);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            TextBox1.MaxLength = Column.MaxLength;
            TextBox1.ToolTip = Column.Description;

            SetUpValidator(RequiredFieldValidator1);
            SetUpValidator(RegularExpressionValidator1);
            SetUpValidator(DynamicValidator1);
        }

        #endregion
    }
}