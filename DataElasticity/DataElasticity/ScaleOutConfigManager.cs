#region usings

using System;
using System.Collections.Generic;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.Unity;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity
{
    /// <summary>
    /// This is a mostly internal utility class for easily interfacing with the 
    /// IConfigRepository and ISettingsRepository
    /// </summary>
    internal class ScaleOutConfigManager : UnityBasedManager<ScaleOutConfigManager>
    {
        #region fields

        private readonly IConfigRepository _configRepository;

        #endregion

        #region constructors

        public ScaleOutConfigManager(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        #endregion

        #region methods

        public Shard AddPointerShardToShardSet(Shard shard, ShardSetConfig shardSetConfig)
        {
            return _configRepository.AddShardToShardSet(shard, shardSetConfig);
        }

        public Server AddServer(Server server)
        {
            return _configRepository.AddServer(server);
        }

        public ShardSetConfig AddShardSetConfig()
        {
            return null;
        }

        public ShardSetConfig GetCurrentShardSetConfig(string tableName)
        {
            return _configRepository.GetShardSetConfigLatestVersion(tableName);
        }

        public Shard GetPointerShard(int pointerShardID)
        {
            return _configRepository.GetShard(pointerShardID);
        }

        public IList<Shard> GetPointerShardsForShardSet(ShardSetConfig shardSetConfig)
        {
            return _configRepository.GetShardsForShardSet(shardSetConfig);
        }

        public Server GetServer(int serverID)
        {
            return _configRepository.GetServer(serverID);
        }

        public Server GetServerByName(string serverName)
        {
            return _configRepository.GetServerByName(serverName);
        }

        public IList<Server> GetServers()
        {
            return _configRepository.GetServers();
        }

        public ShardMap GetShardMap(int shardMapID)
        {
            return _configRepository.GetShardMap(shardMapID);
        }

        public IShardSetDriver GetShardSetDriver(ShardSetConfig shardSetConfig)
        {
            return _container.Resolve<IShardSetDriver>(shardSetConfig.ShardSetName);
        }

        public IList<String> GetTableList()
        {
            return _configRepository.GetShardSetNames();
        }

        public bool IsCurrentShardSetConfig(ShardSetConfig shardSetConfig)
        {
            return shardSetConfig.ShardSetConfigID ==
                   _configRepository.GetCurrentTableShardSetID(shardSetConfig.ShardSetID);
        }

        public Shard ModifyPointerShard(Shard shard)
        {
            return _configRepository.ModifyShard(shard);
        }

        public Server ModifyServer(Server server)
        {
            return _configRepository.ModifyServer(server);
        }

        public void RemovePointerShard(Shard shard)
        {
            _configRepository.RemoveShard(shard);
        }

        public ShardSetConfig SaveConfiguration(ShardSetConfig shardSetConfig)
        {
            return _configRepository.AddShardSetConfig(shardSetConfig);
        }

        #endregion
    }
}