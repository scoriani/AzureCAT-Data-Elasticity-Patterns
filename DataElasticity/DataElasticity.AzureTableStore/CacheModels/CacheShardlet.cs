#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Shards;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels
{
    /// <summary>
    /// Class CacheShardlet is the serializable object held in the object cache for shardlets.
    /// </summary>
    [Serializable]
    public class CacheShardlet
    {
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
        public long DistributionKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CacheShardlet"/> is pinned.
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
        public string ShardSetName { get; set; }

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

        #region methods

        /// <summary>
        /// Convert to the the Azure Table Storage model for azure shardlets.
        /// </summary>
        /// <returns>AzureShardlet.</returns>
        public AzureShardlet ToAzureShardlet()
        {
            if (IsEmpty)
            {
                return new AzureShardlet();
            }
            return new AzureShardlet
            {
                Catalog = Catalog,
                ServerInstanceName = ServerInstanceName,
                Status = Status,
                DistributionKey = DistributionKey,
                ShardingKey = ShardingKey,
                ShardSetName = ShardSetName,
                Pinned = Pinned
            };
        }

        #endregion
    }
}