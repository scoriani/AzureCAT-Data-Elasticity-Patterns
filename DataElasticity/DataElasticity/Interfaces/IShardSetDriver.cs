#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Interfaces
{
    /// <summary>
    /// Interface for working with an individual Shard Set. Each shard set needs an IShardSetDriver implemented for it.
    /// The interface implements how Data Elasticity handles the interfacing with your database.
    /// </summary>
    public interface IShardSetDriver
    {
        #region events

        event TableConfigPublishingHandler ShardSetConfigPublishing;

        #endregion

        #region methods

        /// <summary>
        /// Gets the distribution key for sharding key for the Shard Set.
        /// </summary>
        /// <param name="shardingKey">The sharding key.</param>
        /// <returns>System.Int64.</returns>
        long GetDistributionKeyForShardingKey(string shardingKey);

        /// <summary>
        /// Call to the shard set driver when a shard is to be first created..
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        void CreateShard(ShardBase shard, ShardSetConfig shardSetConfig);

        /// <summary>
        /// Call to the shard set driver when a shard is to be deleted.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        void DeleteShard(ShardBase shard, ShardSetConfig shardSetConfig);

        /// <summary>
        /// Call to the shard set driver when an existing shard is being synchronized.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        void SyncShard(ShardBase shard, ShardSetConfig shardSetConfig);

        /// <summary>
        /// Call to the shard set driver when a shardlet is copied from one shard to another.
        /// </summary>
        /// <param name="sourceShard">The source shard.</param>
        /// <param name="destinationShard">The destination shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        /// <param name="shardingKey">The shardingKey key.</param>
        /// <param name="uniqueProcessID">The unique process identifier.</param>
        void CopyShardlet(ShardBase sourceShard, ShardBase destinationShard, ShardSetConfig shardSetConfig,
            string shardingKey, Guid uniqueProcessID);

        /// <summary>
        /// Call to the shard set driver when a specific shardlet is to be deleted from the shardlet.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        /// <param name="shardingKey">The sharding key.</param>
        void DeleteShardlet(ShardBase shard, ShardSetConfig shardSetConfig, string shardingKey);

        #endregion
    }
}