#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.AzureCat.Patterns.DataElasticity.SQLAzure.Models;
using RangeShard = Microsoft.AzureCat.Patterns.DataElasticity.Models.RangeShard;
using Server = Microsoft.AzureCat.Patterns.DataElasticity.Models.Server;
using Shard = Microsoft.AzureCat.Patterns.DataElasticity.Models.Shard;
using ShardMap = Microsoft.AzureCat.Patterns.DataElasticity.Models.ShardMap;
using ShardSetConfig = Microsoft.AzureCat.Patterns.DataElasticity.Models.ShardSetConfig;
using ShardSetConfigSetting = Microsoft.AzureCat.Patterns.DataElasticity.SQLAzure.Models.ShardSetConfigSetting;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.SQLAzure
{
    public class SQLConfigRepository : IConfigRepository, ISettingsRepository
    {
        #region properties

        public string ConnectionString { get; set; }

        #endregion

        #region constructors

        public SQLConfigRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        #endregion

        #region IConfigRepository

        public Shard AddShardToShardSet(
            Shard shard, ShardSetConfig shardSetConfig)
        {
            using (var context = GetConnection(ConnectionString))
            {
                var dbShard = new Models.Shard
                {
                    Database =
                        context.Databases
                            .FirstOrDefault(
                                d =>
                                    d.DatabaseName == shard.Catalog &&
                                    d.Server.ServerName == shard.ServerInstanceName),
                    Description = shard.Description,
                    ShardSet =
                        context.ShardSets
                            .FirstOrDefault(tg => tg.ShardSetID == shardSetConfig.ShardSetID)
                };

                if (dbShard.Database == null)
                {
                    dbShard.Database =
                        new Database
                        {
                            DatabaseName = shard.Catalog,
                            Server =
                                context.Servers.FirstOrDefault(s => s.ServerName == shard.ServerInstanceName),
                        };

                    context.Databases.Add(dbShard.Database);
                }

                context.Shards.Add(dbShard);
                context.SaveChanges();
                shard.PointerShardID = dbShard.ShardID;
            }

            return shard;
        }

        public Server AddServer(Server server)
        {
            using (var context = GetConnection(ConnectionString))
            {
                var dbServer = context.Servers.FirstOrDefault(s => s.ServerName == server.ServerInstanceName);
                if (dbServer != null)
                {
                    throw new ArgumentException("Server name already in use");
                }

                dbServer =
                    new Models.Server
                    {
                        ServerName = server.ServerInstanceName,
                        Location = server.Location,
                        MaxShardsAllowed = server.MaxShardsAllowed,
                    };

                context.Servers.Add(dbServer);
                context.SaveChanges();

                server.ServerID = dbServer.ServerID;
                server.AvailableShards = server.MaxShardsAllowed;
            }

            return server;
        }

        public ShardMap AddShardMap(ShardMap shardMap)
        {
            ShardMap savedShardMap;
            using (var context = GetConnection(ConnectionString))
            {
                using (var scope = new TransactionScope())
                {
                    savedShardMap = AddShardMap(shardMap, context);
                    scope.Complete();
                }
            }
            return savedShardMap;
        }

        public ShardSetConfig AddShardSetConfig(
            ShardSetConfig shardSetConfig)
        {
            using (var context = GetConnection(ConnectionString))
            {
                using (var scope = new TransactionScope())
                {
                    ShardSet shardSet;

                    // Locate the associated shard set either by the key or the shard set name
                    // If it does not exist, create and save it.

                    // Check to see if they gave us a ShardSetID
                    if (shardSetConfig.ShardSetID == -1)
                    {
                        // Lets try to look it up by Name
                        shardSet =
                            context.ShardSets.FirstOrDefault(x => x.Name == shardSetConfig.ShardSetName);

                        //If it doesn't exist create it.
                        if (shardSet == null)
                        {
                            shardSet =
                                new ShardSet
                                {
                                    Name = shardSetConfig.ShardSetName
                                };

                            context.ShardSets.Add(shardSet);
                        }
                    }
                    else
                    {
                        shardSet =
                            context.ShardSets.FirstOrDefault(
                                x => x.ShardSetID == shardSetConfig.ShardSetID);

                        if (shardSet == null)
                            throw new InvalidOperationException(string.Format("Shard Set ID {0} does not exist",
                                shardSetConfig.ShardSetID));
                    }


                    // if the configuration is being saved with a new Id, record it on the config table
                    if (shardSetConfig.CurrentPublishedShardMapID.HasValue)
                    {
                        shardSet.CurrentShardMapID = shardSetConfig.CurrentPublishedShardMapID;
                    }

                    // Nuke the server list because we are going to reload it from the configuration
                    shardSet.Servers.Clear();

                    //Now read the servers
                    foreach (var server in shardSetConfig.Servers)
                    {
                        //is the server new?
                        if (server.ServerID == -1)
                        {
                            server.Save();
                        }

                        //Lets grab the server object from the DB.
                        var dbServer = context.Servers.FirstOrDefault(x => x.ServerID == server.ServerID);
                        if (dbServer == null)
                        {
                            throw new Exception("Server not found in the config database");
                        }
                        shardSet.Servers.Add(dbServer);
                    }

                    if (shardSetConfig.ShardMap.ShardMapID == -1)
                    {
                        shardSetConfig.ShardMap = AddShardMap(shardSetConfig.ShardMap, context);
                    }

                    var dbShardSetConfig =
                        new Models.ShardSetConfig
                        {
                            AllowDeployment = shardSetConfig.AllowDeployments,
                            MaxShardCount = shardSetConfig.MaxShardCount,
                            MaxShardSizeMB = shardSetConfig.MaxShardSizeMb,
                            MaxShardletsPerShard = shardSetConfig.MaxShardletsPerShard,
                            MinShardSizeMB = shardSetConfig.MinShardSizeMb,
                            TargetShardCount = shardSetConfig.TargetShardCount,
                            ShardMapID = shardSetConfig.ShardMap.ShardMapID,
                            ShardSet = shardSet,
                            Version = (shardSet.ShardSetConfigs.Count() + 1)
                        };

                    // create settings on the new configuration
                    foreach (var shardSetConfigSetting in shardSetConfig.ShardSetConfigSettings)
                    {
                        var setting = new ShardSetConfigSetting
                        {
                            SettingKey = shardSetConfigSetting.SettingKey,
                            SettingValue = shardSetConfigSetting.SettingValue,
                            ShardSetConfig = dbShardSetConfig
                        };

                        dbShardSetConfig.ShardSetConfigSettings.Add(setting);
                    }

                    context.ShardSetConfigs.Add(dbShardSetConfig);
                    context.SaveChanges();

                    shardSetConfig.ShardSetConfigID = dbShardSetConfig.ShardSetConfigID;
                    shardSetConfig.ShardSetID = shardSet.ShardSetID;

                    AddPointerShards(context, shardSetConfig);

                    scope.Complete();
                }
            }

            return shardSetConfig;
        }

        public int GetCurrentTableShardSetID(int shardSetID)
        {
            var returnVal = 0;
            using (var context = GetConnection(ConnectionString))
            {
                var shardSetConfigID =
                    context.ShardSetConfigs
                        .Where(x => x.ShardSet.ShardSetID == shardSetID)
                        .OrderByDescending(x => x.Version)
                        .Select(x => x.ShardSetConfigID)
                        .FirstOrDefault();

                if (shardSetConfigID != 0)
                {
                    returnVal = shardSetConfigID;
                }
            }

            return returnVal;
        }

        public Shard GetShard(int pointerShardID)
        {
            using (var context = GetConnection(ConnectionString))
            {
                var pointerShard = context.Shards.FirstOrDefault(ps => ps.ShardID == pointerShardID);

                return new Shard
                {
                    Catalog = pointerShard.Database.DatabaseName,
                    Description = pointerShard.Description,
                    ServerInstanceName = pointerShard.Database.Server.ServerName,
                    PointerShardID = pointerShard.ShardID
                };
            }
        }

        public IList<Shard> GetShardsForShardSet(
            ShardSetConfig shardSetConfig)
        {
            using (var context = GetConnection(ConnectionString))
            {
                return context.Shards.Where(ps => ps.ShardSetID == shardSetConfig.ShardSetID)
                    .Select(ps =>
                        new Shard
                        {
                            Catalog = ps.Database.DatabaseName,
                            Description = ps.Description,
                            ServerInstanceName = ps.Database.Server.ServerName,
                            PointerShardID = ps.ShardID
                        })
                    .ToList();
            }
        }

        public Server GetServer(int serverID)
        {
            using (var context = GetConnection(ConnectionString))
            {
                return context.Servers
                    .Where(s => s.ServerID == serverID)
                    .Select(s =>
                        new Server
                        {
                            Location = s.Location,
                            ServerInstanceName = s.ServerName,
                            MaxShardsAllowed = s.MaxShardsAllowed,
                            AvailableShards = s.MaxShardsAllowed - s.Databases.Count,
                            //TODO : Change to databases that are deployed or planned.. not all databases in history
                            ServerID = s.ServerID,
                        })
                    .FirstOrDefault();
            }
        }

        public Server GetServerByName(string serverName)
        {
            using (var context = GetConnection(ConnectionString))
            {
                return
                    context.Servers.Where(s => s.ServerName == serverName)
                        .Select(s =>
                            new Server
                            {
                                Location = s.Location,
                                ServerInstanceName = s.ServerName,
                                MaxShardsAllowed = s.MaxShardsAllowed,
                                AvailableShards = s.MaxShardsAllowed - s.Databases.Count,
                                //TODO : Change to databases that are deployed or planned.. not all databases in history
                                ServerID = s.ServerID,
                            })
                        .FirstOrDefault();
            }
        }


        public IList<Server> GetServers()
        {
            using (var context = GetConnection(ConnectionString))
            {
                var servers =
                    context.Servers
                        .Select(s =>
                            new Server
                            {
                                Location = s.Location,
                                ServerInstanceName = s.ServerName,
                                MaxShardsAllowed = s.MaxShardsAllowed,
                                AvailableShards = s.MaxShardsAllowed,
                                // TODO: Lookup how many shards are on the node now.
                                ServerID = s.ServerID,
                            })
                        .ToList();

                return servers;
            }
        }

        public ShardMap GetShardMap(int shardMapID)
        {
            ShardMap map = null;

            using (var context = GetConnection(ConnectionString))
            {
                var dbShardMap = context.ShardMaps.FirstOrDefault(x => x.ShardMapID == shardMapID);
                if (dbShardMap != null)
                {
                    map = new ShardMap
                    {
                        ShardMapID = dbShardMap.ShardMapID,
                        Shards = dbShardMap.RangeShards.Select(x => new RangeShard
                        {
                            ServerInstanceName = x.Database.Server.ServerName,
                            Catalog = x.Database.DatabaseName,
                            HighDistributionKey = x.RangeHighValue,
                            LowDistributionKey = x.RangeLowValue,
                            ShardID = x.ShardID
                        }).ToList()
                    };
                }
            }
            return map;
        }

        public ShardSetConfig GetShardSetConfigLatestVersion(string shardSetName)
        {
            ShardSetConfig returnVal = null;
            using (var context = GetConnection(ConnectionString))
            {
                var shardSetConfig =
                    context.ShardSetConfigs
                        .Where(x => x.ShardSet.Name == shardSetName)
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefault();

                if (shardSetConfig != null)
                    returnVal = MakeShardSetConfig(shardSetConfig);
            }
            return returnVal;
        }

        public IList<ShardSetConfig> GetShardSetConfigs(string shardSetName)
        {
            var returnVal = new List<ShardSetConfig>();
            using (var context = GetConnection(ConnectionString))
            {
                var shardSetConfigs =
                    context.ShardSetConfigs
                        .Where(tgc => tgc.ShardSet.Name == shardSetName);

                foreach (var shardSetConfig in shardSetConfigs)
                {
                    var newShardSetConfig = MakeShardSetConfig(shardSetConfig);
                    returnVal.Add(newShardSetConfig);
                }
            }

            return returnVal;
        }

        public IList<string> GetShardSetNames()
        {
            using (var context = GetConnection(ConnectionString))
            {
                return context.ShardSets.Select(tg => tg.Name).ToList();
            }
        }

        public Shard ModifyShard(Shard shard)
        {
            using (var context = GetConnection(ConnectionString))
            {
                var dbShard =
                    context.Shards.FirstOrDefault(ps => ps.ShardID == shard.PointerShardID);
                if (dbShard == null)
                {
                    throw new Exception("Shard not found.");
                }

                dbShard.Database =
                    context.Databases
                        .FirstOrDefault(
                            x =>
                                x.DatabaseName == shard.Catalog &&
                                x.Server.ServerName == shard.ServerInstanceName);
                if (dbShard.Database == null)
                {
                    dbShard.Database =
                        new Database
                        {
                            DatabaseName = shard.Catalog,
                            Server =
                                context.Servers.FirstOrDefault(x => x.ServerName == shard.ServerInstanceName),
                        };

                    context.Databases.Add(dbShard.Database);
                }
                dbShard.Description = shard.Description;

                context.SaveChanges();
            }

            return shard;
        }

        public Server ModifyServer(Server server)
        {
            using (var context = GetConnection(ConnectionString))
            {
                var dbServer = context.Servers.FirstOrDefault(s => s.ServerID == server.ServerID);
                if (dbServer == null)
                {
                    throw new NullReferenceException("Server not found");
                }

                dbServer.Location = server.Location;
                dbServer.MaxShardsAllowed = server.MaxShardsAllowed;
                dbServer.ServerName = server.ServerInstanceName;

                context.SaveChanges();
            }

            return server;
        }

        public void RemoveShard(Shard shard)
        {
            using (var context = GetConnection(ConnectionString))
            {
                var dbShard = context.Shards.FirstOrDefault(x => x.ShardID == shard.PointerShardID);
                if (dbShard == null)
                {
                    throw new Exception("Shard not found.");
                }

                context.Shards.Remove(dbShard);
                context.SaveChanges();
            }
        }

        #endregion

        #region ISettingsRepository

        public Settings GetSettings()
        {
            using (var context = GetConnection(ConnectionString))
            {
                var setting = context.Settings.OrderByDescending(s => s.Version).FirstOrDefault();
                if (setting != null)
                {
                    return new Settings
                    {
                        AdminPassword = Encoding.UTF8.GetString(setting.AdminUserPassword),
                        ShardPassword = Encoding.UTF8.GetString(setting.ShardUserPassword),
                        // TODO : Decided where security goes and implement some kind of decryption.
                        ShardPrefix = setting.ShardPrefix,
                        ShardUser = setting.ShardUserName,
                        AdminUser = setting.AdminUserName,
                    };
                }
                throw new NullReferenceException("Settings not configured yet");
            }
        }

        public Settings SaveSettings(Settings settings)
        {
            using (var context = GetConnection(ConnectionString))
            {
                var setting = new Setting
                {
                    Version = context.Settings.Count() + 1,
                    DateCreated = DateTime.UtcNow,
                    ShardPrefix = settings.ShardPrefix,
                    AdminUserName = settings.AdminUser,
                    AdminUserPassword = Encoding.UTF8.GetBytes(settings.AdminPassword),
                    ShardUserName = settings.ShardUser,
                    ShardUserPassword = Encoding.UTF8.GetBytes(settings.ShardPassword),
                    // TODO : Decided where security goes and implement some kind of encryption.
                };

                context.Settings.Add(setting);
                context.SaveChanges();
            }
            return settings;
        }

        #endregion

        #region methods

        private void AddPointerShards(DataElasticityEntities context, ShardSetConfig shardSetConfig)
        {
            foreach (var ps in shardSetConfig.Shards)
            {
                if (ps.PointerShardID == -1)
                    AddShardToShardSet(ps, shardSetConfig);
                else
                    ModifyShard(ps);
            }

            var deleteShards =
                context.Shards.Where(p => p.ShardSetID == shardSetConfig.ShardSetID);

            foreach (var ps in deleteShards)
            {
                if (shardSetConfig.Shards.Count(x => x.PointerShardID == ps.ShardID) == 0)
                    context.Shards.Remove(ps);
            }
        }

        private ShardMap AddShardMap(ShardMap shardMap, DataElasticityEntities context)
        {
            if (shardMap.ShardMapID != -1)
            {
                throw new ArgumentException("Shard Map must be new");
            }
            if (shardMap.Shards.Any(x => x.ShardID != -1))
            {
                throw new ArgumentException("All shards in map must be new");
            }

            var dbShardMap = new Models.ShardMap();
            context.ShardMaps.Add(dbShardMap);
            context.SaveChanges();
            shardMap.ShardMapID = dbShardMap.ShardMapID;
            foreach (var shard in shardMap.Shards)
            {
                var dbShard = new Models.RangeShard
                {
                    Database =
                        context.Databases
                            .FirstOrDefault(
                                d => d.DatabaseName == shard.Catalog && d.Server.ServerName == shard.ServerInstanceName),
                    RangeHighValue = shard.HighDistributionKey,
                    RangeLowValue = shard.LowDistributionKey,
                    ShardMap = dbShardMap,
                };

                if (dbShard.Database == null)
                {
                    dbShard.Database = new Database
                    {
                        DatabaseName = shard.Catalog,
                        Server = context.Servers.FirstOrDefault(s => s.ServerName == shard.ServerInstanceName),
                    };

                    context.Databases.Add(dbShard.Database);
                }

                context.RangeShards.Add(dbShard);
                context.SaveChanges();

                shard.ShardID = dbShard.ShardID;
            }
            return shardMap;
        }

        private DataElasticityEntities GetConnection(string connectionString)
        {
            return EfShardConnectionBuilder
                .MakeContext<DataElasticityEntities>(connectionString, "Models.DataElasticityConfig");
        }

        private ShardSetConfig MakeShardSetConfig(Models.ShardSetConfig shardSetConfigModel)
        {
            var shardSetConfig =
                new ShardSetConfig
                {
                    ShardSetConfigID = shardSetConfigModel.ShardSetConfigID,
                    ShardSetID = shardSetConfigModel.ShardSetID,
                    AllowDeployments = shardSetConfigModel.AllowDeployment,
                    CurrentShardCount = shardSetConfigModel.ShardMap.RangeShards.Count(),
                    MaxShardCount = shardSetConfigModel.MaxShardCount,
                    MaxShardSizeMb = shardSetConfigModel.MaxShardSizeMB,
                    MaxShardletsPerShard = shardSetConfigModel.MaxShardletsPerShard,
                    MinShardSizeMb = shardSetConfigModel.MinShardSizeMB,
                    TargetShardCount = shardSetConfigModel.TargetShardCount,
                    Version = shardSetConfigModel.Version,
                    ShardSetName = shardSetConfigModel.ShardSet.Name,
                    CurrentPublishedShardMapID = shardSetConfigModel.ShardSet.CurrentShardMapID,
                    ShardMap = new ShardMap
                    {
                        ShardMapID = shardSetConfigModel.ShardMapID,
                        Shards =
                            shardSetConfigModel.ShardMap.RangeShards
                                .Select(s =>
                                    new RangeShard
                                    {
                                        ServerInstanceName = s.Database.Server.ServerName,
                                        Catalog = s.Database.DatabaseName,
                                        HighDistributionKey = s.RangeHighValue,
                                        LowDistributionKey = s.RangeLowValue,
                                        ShardID = s.ShardID
                                    })
                                .ToList()
                    },
                    Shards = shardSetConfigModel.ShardSet.Shards
                        .Select(x =>
                            new Shard
                            {
                                Catalog = x.Database.DatabaseName,
                                ServerInstanceName = x.Database.Server.ServerName,
                                Description = x.Description,
                                PointerShardID = x.ShardID
                            })
                        .ToList(),
                    Servers = shardSetConfigModel.ShardSet.Servers
                        .Select(x =>
                            new Server
                            {
                                ServerID = x.ServerID,
                                ServerInstanceName = x.ServerName,
                                MaxShardsAllowed = x.MaxShardsAllowed,
                                AvailableShards = x.MaxShardsAllowed - x.Databases.Count
                                //TODO : Change to databases that are deployed or planned.. not all databases in history
                            })
                        .ToList(),
                    ShardSetConfigSettings = shardSetConfigModel.ShardSetConfigSettings
                        .Select(x =>
                            new DataElasticity.Models.ShardSetConfigSetting
                            {
                                ShardSetConfigSettingID = x.ShardSetConfigSettingID,
                                ShardSetConfigID = x.ShardSetConfigID,
                                SettingKey = x.SettingKey,
                                SettingValue = x.SettingValue
                            })
                        .ToList(),
                };

            return shardSetConfig;
        }

        #endregion
    }
}