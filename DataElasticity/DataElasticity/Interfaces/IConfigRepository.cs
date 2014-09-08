#region usings

using System;
using System.Collections.Generic;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Interfaces
{
    /// <summary>
    /// IConfigRepository provides the common Interface to  storage of configuration data for the sharding system
    /// </summary>
    public interface IConfigRepository
    {
        #region methods

        Server AddServer(Server server);
        ShardMap AddShardMap(ShardMap shardMap);
        ShardSetConfig AddShardSetConfig(ShardSetConfig shardSetConfig);
        Shard AddShardToShardSet(Shard shard, ShardSetConfig shardSetConfig);
        int GetCurrentTableShardSetID(int shardSetID);
        Server GetServer(int serverID);
        Server GetServerByName(string serverName);
        IList<Server> GetServers();
        Shard GetShard(int pointerShardID);
        ShardMap GetShardMap(int shardMapID);
        ShardSetConfig GetShardSetConfigLatestVersion(String shardSetName);
        IList<ShardSetConfig> GetShardSetConfigs(String shardSetName);
        IList<String> GetShardSetNames();
        IList<Shard> GetShardsForShardSet(ShardSetConfig shardSetConfig);
        Server ModifyServer(Server server);
        Shard ModifyShard(Shard shard);
        void RemoveShard(Shard shard);

        #endregion
    }
}