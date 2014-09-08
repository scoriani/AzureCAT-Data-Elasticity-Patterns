#region usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Web.DynamicData;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates
{
    public partial class ManyToMany_EditField : FieldTemplateUserControl
    {
        #region properties

        public override Control DataControl
        {
            get { return CheckBoxList1; }
        }

        protected ObjectContext ObjectContext { get; set; }

        #endregion

        #region methods

        public void Page_Load(object sender, EventArgs e)
        {
            var ds = (EntityDataSource) this.FindDataSourceControl();

            ds.ContextCreated += (_, ctxCreatedEnventArgs) => ObjectContext = ctxCreatedEnventArgs.Context;

            ds.Updating += DataSource_UpdatingOrInserting;
            ds.Inserting += DataSource_UpdatingOrInserting;
        }

        protected void CheckBoxList1_DataBound(object sender, EventArgs e)
        {
            var childTable = ChildrenColumn.ChildTable;

            IEnumerable<object> entityCollection = null;

            if (Mode == DataBoundControlMode.Edit)
            {
                object entity;
                var rowDescriptor = Row as ICustomTypeDescriptor;
                if (rowDescriptor != null)
                {
                    entity = rowDescriptor.GetPropertyOwner(null);
                }
                else
                {
                    entity = Row;
                }

                entityCollection = (IEnumerable<object>) Column.EntityTypeProperty.GetValue(entity, null);
                var realEntityCollection = entityCollection as RelatedEnd;
                if (realEntityCollection != null && !realEntityCollection.IsLoaded)
                {
                    realEntityCollection.Load();
                }
            }

            foreach (var childEntity in childTable.GetQuery(ObjectContext))
            {
                var listItem = new ListItem(
                    childTable.GetDisplayString(childEntity),
                    childTable.GetPrimaryKeyString(childEntity));

                if (Mode == DataBoundControlMode.Edit)
                {
                    listItem.Selected = ListContainsEntity(childTable, entityCollection, childEntity);
                }
                CheckBoxList1.Items.Add(listItem);
            }
        }

        private static bool AreEntitiesEqual(MetaTable table, object entity1, object entity2)
        {
            return table.GetPrimaryKeyValues(entity1).SequenceEqual(table.GetPrimaryKeyValues(entity2));
        }

        private void DataSource_UpdatingOrInserting(object sender, EntityDataSourceChangingEventArgs e)
        {
            var childTable = ChildrenColumn.ChildTable;

            if (Mode == DataBoundControlMode.Edit)
            {
                ObjectContext.LoadProperty(e.Entity, Column.Name);
            }

            dynamic entityCollection = Column.EntityTypeProperty.GetValue(e.Entity, null);

            foreach (dynamic childEntity in childTable.GetQuery(e.Context))
            {
                var isCurrentlyInList = ListContainsEntity(childTable, entityCollection, childEntity);

                string pkString = childTable.GetPrimaryKeyString(childEntity);
                var listItem = CheckBoxList1.Items.FindByValue(pkString);
                if (listItem == null)
                    continue;

                if (listItem.Selected)
                {
                    if (!isCurrentlyInList)
                        entityCollection.Add(childEntity);
                }
                else
                {
                    if (isCurrentlyInList)
                        entityCollection.Remove(childEntity);
                }
            }
        }

        private static bool ListContainsEntity(MetaTable table, IEnumerable<object> list, object entity)
        {
            return list.Any(e => AreEntitiesEqual(table, e, entity));
        }

        #endregion
    }
}