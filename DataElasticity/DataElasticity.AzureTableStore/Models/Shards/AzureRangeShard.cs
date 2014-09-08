#region usings

using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Shards
{
    /// <summary>
    /// Class AzureRangeShard is the entity object for a Range Shard in the Azure Table Store.
    /// </summary>
    public class AzureRangeShard : LongBasedRowKeyEntity
    {
        #region constants

        /// <summary>
        /// The range shard table name
        /// </summary>
        public const string RangeShardTable = "rangeshardtable";

        #endregion

        #region fields

        private long _maxRange;

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the catalog.
        /// </summary>
        /// <value>The catalog.</value>
        public string Catalog { get; set; }

        /// <summary>
        /// Gets or sets the maximum range.
        /// </summary>
        /// <value>The maximum range.</value>
        public long MaxRange
        {
            get { return _maxRange; }
            set
            {
                _maxRange = value;
                RowKey = MakeRowKeyFromLong(_maxRange);
            }
        }

        /// <summary>
        /// Gets or sets the name of the server instance.
        /// </summary>
        /// <value>The name of the server instance.</value>
        public string ServerInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the shard set.
        /// </summary>
        /// <value>The name of the shard set.</value>
        public string ShardSetName
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> class with the specified partition key and row key.
        /// </summary>
        /// <param name="partitionKey">A string containing the partition key of the <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> to be initialized.</param>
        /// <param name="rowKey">A string containing the row key of the <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> to be initialized.</param>
        public AzureRangeShard(string partitionKey, string rowKey) :
            base(partitionKey, rowKey)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureRangeShard"/> class.
        /// </summary>
        /// <param name="rangeShard">The range shard.</param>
        /// <param name="shardSetName">Name of the shard set.</param>
        public AzureRangeShard(RangeShard rangeShard, string shardSetName)
        {
            Catalog = rangeShard.Catalog;
            MaxRange = rangeShard.HighDistributionKey;
            ServerInstanceName = rangeShard.ServerInstanceName;
            ShardSetName = shardSetName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureRangeShard"/> class.
        /// </summary>
        public AzureRangeShard()
        {
        }

        #endregion

        #region methods

        /// <summary>
        /// Converts this instance to the cache range shard.
        /// </summary>
        /// <returns>CacheRangeShard.</returns>
        public CacheRangeShard ToCacheRangeShard()
        {
            return new CacheRangeShard
            {
                Catalog = Catalog,
                IsEmpty = false,
                MaxRange = MaxRange,
                ServerInstanceName = ServerInstanceName,
                ShardSetName = ShardSetName
            };
        }

        #endregion
    }
}