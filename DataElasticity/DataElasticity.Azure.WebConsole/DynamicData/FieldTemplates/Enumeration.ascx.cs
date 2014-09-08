#region usings

using System;
using System.Web.DynamicData;
using System.Web.UI;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class EnumerationField : FieldTemplateUserControl
    {
        #region properties

        public override Control DataControl
        {
            get { return Literal1; }
        }

        public string EnumFieldValueString
        {
            get
            {
                if (FieldValue == null)
                {
                    return FieldValueString;
                }

                var enumType = Column.GetEnumType();
                if (enumType != null)
                {
                    var enumValue = Enum.ToObject(enumType, FieldValue);
                    return FormatFieldValue(enumValue);
                }

                return FieldValueString;
            }
        }

        #endregion
    }
}