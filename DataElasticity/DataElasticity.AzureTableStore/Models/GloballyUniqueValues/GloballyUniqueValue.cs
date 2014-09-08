#region usings

using System.Runtime.Serialization;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels;
using Microsoft.WindowsAzure.Storage.Table.DataServices;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.GloballyUniqueValues
{
    public class GloballyUniqueValue : TableServiceEntity
    {
        #region properties

        [DataMember]
        public string DataSetName //Typically the name of the table in the database
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        [DataMember]
        public long DistributionKey { get; set; }

        [DataMember]
        public string UniqueValue
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        #endregion

        #region constructors

        public GloballyUniqueValue(string partitionKey, string rowKey) :
            base(partitionKey, rowKey)
        {
        }

        public GloballyUniqueValue()
        {
        }

        #endregion

        #region methods

        public CacheGloballyUniqueValue ToCacheGloballyUniqueValue()
        {
            return new CacheGloballyUniqueValue
            {
                IsEmpty = false,
                DataSetName = DataSetName,
                UniqueValue = UniqueValue,
                DistributionKey = DistributionKey
            };
        }

        #endregion
    }
}