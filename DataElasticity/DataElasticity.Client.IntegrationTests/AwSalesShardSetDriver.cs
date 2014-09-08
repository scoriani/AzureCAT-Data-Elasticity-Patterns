#region usings

using System;
using Microsoft.AzureCat.Patterns.CityHash;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Client.IntegrationTests
{
    internal class AwSalesShardSetDriver : IShardSetDriver
    {
        #region IShardSetDriver

        /// <summary>
        /// Call to the shard set driver when a shardlet is copied from one shard to another.
        /// </summary>
        /// <param name="sourceShard">The source shard.</param>
        /// <param name="destinationShard">The destination shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        /// <param name="shardingKey">The distribution key.</param>
        /// <param name="uniqueProcessID">The unique process identifier.</param>
        public void CopyShardlet(ShardBase sourceShard, ShardBase destinationShard, ShardSetConfig shardSetConfig,
            string shardingKey, Guid uniqueProcessID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Call to the shard set driver when a shard is to be first created..
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        public void CreateShard(ShardBase shard, ShardSetConfig shardSetConfig)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Call to the shard set driver when a shard is to be deleted.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        public void DeleteShard(ShardBase shard, ShardSetConfig shardSetConfig)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Call to the shard set driver when a specific shardlet is to be deleted from the shardlet.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        /// <param name="shardingKey">The distribution key.</param>
        public void DeleteShardlet(ShardBase shard, ShardSetConfig shardSetConfig, string shardingKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the distribution key for sharding key for the DHard Set.
        /// </summary>
        /// <param name="shardingKey">The sharding key.</param>
        /// <returns>System.Int64.</returns>
        public long GetDistributionKeyForShardingKey(string shardingKey)
        {
            return (long)CityHasher.CityHash64String(shardingKey);
        }

        public event TableConfigPublishingHandler ShardSetConfigPublishing;

        /// <summary>
        /// Call to the shard set driver when an existing shard is being synchronized.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        public void SyncShard(ShardBase shard, ShardSetConfig shardSetConfig)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}