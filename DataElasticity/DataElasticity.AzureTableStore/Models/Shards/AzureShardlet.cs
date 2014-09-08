#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Shards
{
    /// <summary>
    /// Class AzureShardlet is the data model class for saving data into the shardlet map in azure table storage.
    /// </summary>
    public class AzureShardlet : LongBasedRowKeyEntity
    {
        #region constants

        /// <summary>
        /// The shardlet map table name
        /// </summary>
        public const string ShardletMap = "shardletmap";

        #endregion

        #region fields

        /// <summary>
        /// The _distribution key
        /// </summary>
        private long _distributionKey;

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the catalog.
        /// </summary>
        /// <value>The catalog.</value>
        public string Catalog { get; set; }

        /// <summary>
        /// Gets or sets the distribution key.
        /// </summary>
        /// <value>The distribution key.</value>
        public long DistributionKey
        {
            get { return _distributionKey; }
            set
            {
                _distributionKey = value;
                RowKey = MakeRowKeyFromLong(_distributionKey);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AzureShardlet"/> is pinned.
        /// </summary>
        /// <value><c>true</c> if pinned; otherwise, <c>false</c>.</value>
        public bool Pinned { get; set; }

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

        /// <summary>
        /// Gets or sets the sharding key.
        /// </summary>
        /// <value>The sharding key.</value>
        public string ShardingKey { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public string Status { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureShardlet"/> class.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        public AzureShardlet(string partitionKey, string rowKey) :
            base(partitionKey, rowKey)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureShardlet"/> class.
        /// </summary>
        public AzureShardlet()
        {
        }

        #endregion

        #region methods

        /// <summary>
        /// Gets the row key.
        /// </summary>
        /// <param name="distributionKey">The distribution key.</param>
        /// <returns>System.String.</returns>
        public static string GetRowKey(long distributionKey)
        {
            return MakeRowKeyFromLong(distributionKey);
        }

        /// <summary>
        /// Return a cached ShardLet object form the Azure table store shardlet row
        /// </summary>
        /// <returns>CacheShardlet.</returns>
        public CacheShardlet ToCacheShardlet()
        {
            return new CacheShardlet
            {
                Catalog = Catalog,
                ServerInstanceName = ServerInstanceName,
                IsEmpty = false,
                Status = Status,
                DistributionKey = DistributionKey,
                ShardSetName = ShardSetName,
                ShardingKey = ShardingKey,
                Pinned = Pinned,
            };
        }

        /// <summary>
        /// Return a framework level ShardLet object form the Azure table store shardlet row
        /// </summary>
        /// <returns>The framework level Shardlet.</returns>
        public Shardlet ToFrameworkShardlet()
        {
            return new Shardlet
            {
                Catalog = Catalog,
                ServerInstanceName = ServerInstanceName,
                Status = (ShardletStatus) Enum.Parse(typeof (ShardletStatus), Status),
                DistributionKey = DistributionKey,
                ShardingKey = ShardingKey,
                ShardSetName = ShardSetName,
                Pinned = Pinned
            };
        }

        #endregion
    }
}