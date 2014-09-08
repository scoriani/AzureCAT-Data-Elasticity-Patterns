#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Shards;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels
{
    /// <summary>
    /// Class AzureShardletConnection is the data model class for saving data into the shardlet connections in azure table storage.
    /// </summary>
    [Serializable]
    public class CacheShardletConnection
    {
        #region properties

        /// <summary>
        /// Gets or sets the catalog.
        /// </summary>
        /// <value>The catalog.</value>
        public string Catalog { get; set; }

        /// <summary>
        /// Gets or sets the name of the shard set.
        /// </summary>
        /// <value>The name of the shard set.</value>
        public long DistributionKey { get; set; }

        /// <summary>
        /// Gets or sets the sharding key.
        /// </summary>
        /// <value>The sharding key.</value>
        public string ShardingKey { get; set; }

        /// <summary>
        /// Gets or sets the spid.
        /// </summary>
        /// <value>The spid.</value>
        public int Spid { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Convert to the the Azure Table Storage model for azure shardlet connections.
        /// </summary>
        /// <returns>AzureShardletConnection.</returns>
        public AzureShardletConnection ToAzureShardletConnection()
        {
            return new AzureShardletConnection
            {
                Catalog = Catalog,
                DistributionKey = DistributionKey,
                ShardingKey = ShardingKey,
                Spid = Spid,
            };
        }

        #endregion
    }
}