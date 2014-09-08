#region usings

using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.GloballyUniqueValues
{
    internal class GloballyUniqueValueContext : TableServiceContext
    {
        #region constants

        public const string GloballyUniqueValueTable = "globallyuniquevaluetable";

        #endregion

        #region properties

        public IQueryable<GloballyUniqueValue> uniqueValues
        {
            get { return CreateQuery<GloballyUniqueValue>(GloballyUniqueValueTable); }
        }

        #endregion

        #region constructors

        public GloballyUniqueValueContext(CloudTableClient client)
            : base(client)
        {
            IgnoreResourceNotFoundException = true;
        }

        #endregion
    }
}