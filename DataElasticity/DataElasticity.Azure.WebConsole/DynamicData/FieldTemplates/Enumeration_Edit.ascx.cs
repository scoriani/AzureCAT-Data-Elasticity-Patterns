#region usings

using System;
using System.Collections.Specialized;
using System.Web.DynamicData;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class Enumeration_EditField : FieldTemplateUserControl
    {
        #region fields

        private Type _enumType;

        #endregion

        #region properties

        public override Control DataControl
        {
            get { return DropDownList1; }
        }

        private Type EnumType
        {
            get
            {
                if (_enumType == null)
                {
                    _enumType = Column.GetEnumType();
                }
                return _enumType;
            }
        }

        #endregion

        #region methods

        protected override void ExtractValues(IOrderedDictionary dictionary)
        {
            var value = DropDownList1.SelectedValue;
            if (value == String.Empty)
            {
                value = null;
            }
            dictionary[Column.Name] = ConvertEditedValue(value);
        }

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);

            if (Mode == DataBoundControlMode.Edit && FieldValue != null)
            {
                var selectedValueString = GetSelectedValueString();
                var item = DropDownList1.Items.FindByValue(selectedValueString);
                if (item != null)
                {
                    DropDownList1.SelectedValue = selectedValueString;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DropDownList1.ToolTip = Column.Description;

            if (DropDownList1.Items.Count == 0)
            {
                if (Mode == DataBoundControlMode.Insert || !Column.IsRequired)
                {
                    DropDownList1.Items.Add(new ListItem("[Not Set]", String.Empty));
                }
                PopulateListControl(DropDownList1);
            }

            SetUpValidator(RequiredFieldValidator1);
            SetUpValidator(DynamicValidator1);
        }

        #endregion
    }
}