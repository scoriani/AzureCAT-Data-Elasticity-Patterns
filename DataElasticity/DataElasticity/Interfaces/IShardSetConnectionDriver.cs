#region usings

using System;
using System.Collections.Generic;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Interfaces
{
    /// <summary>
    /// Interface to looking up and handling Shardlet mapping and connection data
    /// </summary>
    public interface IShardSetConnectionDriver
    {
        #region methods

        /// <summary>
        /// Gets the Shardlet by a unique value.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="dataSetName">Name of the data set.</param>
        /// <param name="uniqueValue">The unique value.</param>
        /// <returns>Shardlet.</returns>
        Shardlet GetShardlet(string shardSetName, string dataSetName, string uniqueValue);

        /// <summary>
        /// Gets the current Shardlet by distribution key.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="distributionKey">The distribution key.</param>
        /// <param name="isNew">if set to <c>true</c> the key is a new key and implementer should not look for existing ShardLet.</param>
        /// <returns>Shardlet.</returns>
        Shardlet GetShardletByDistributionKey(string shardSetName, long distributionKey, bool isNew = false);

        /// <summary>
        /// Gets the shardlet by sharding key.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="shardingKey">The sharding key.</param>
        /// <param name="distributionKey">The distribution key.</param>
        /// <returns>Shardlet.</returns>
        Shardlet GetShardletByShardingKey(string shardSetName, string shardingKey, long distributionKey);

        /// <summary>
        /// Gets the shardlets in the shard set.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <returns>IEnumerable&lt;Shardlet&gt;.</returns>
        IEnumerable<Shardlet> GetShardlets(string shardSetName);

        /// <summary>
        /// Publishes the shard into the live shard map.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="rangeShard">The shard.</param>
        void PublishShard(string shardSetName, RangeShard rangeShard);

        /// <summary>
        /// Publishes the Shardlet into the pinned Shardlet list.
        /// </summary>
        /// <param name="shardlet">The Shardlet.</param>
        void PublishShardlet(Shardlet shardlet);

        /// <summary>
        /// Publishes the shardlet connection.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <param name="spid">The spid.</param>
        void PublishShardletConnection(Shardlet shardlet, short spid);

        /// <summary>
        /// Removes the shard from the live shard map.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="rangeShard">The shard.</param>
        void RemoveShard(string shardSetName, RangeShard rangeShard);

        /// <summary>
        /// Removes the shardlet connection.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <param name="spid">The spid.</param>
        void RemoveShardletConnection(Shardlet shardlet, short spid);

        /// <summary>
        /// Terminates the connections to the shardlet.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        void TerminateConnections(Shardlet shardlet);

        #endregion
    }
}