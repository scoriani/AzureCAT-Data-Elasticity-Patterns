#region usings

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    public class ShardSetConfig
    {
        #region properties

        public bool AllowDeployments { get; set; }
        public int? CurrentPublishedShardMapID { get; set; }
        public int CurrentShardCount { get; set; }

        public bool IsCurrent
        {
            get { return ScaleOutConfigManager.GetManager().IsCurrentShardSetConfig(this); }
        }

        public int MaxShardCount { get; set; }
        public int MaxShardSizeMb { get; set; }
        public long MaxShardletsPerShard { get; set; }
        public int MinShardSizeMb { get; set; }
        public IList<Server> Servers { get; set; }
        public ShardMap ShardMap { get; set; }
        public int ShardSetConfigID { get; set; }
        public IList<ShardSetConfigSetting> ShardSetConfigSettings { get; set; }
        public int ShardSetID { get; set; }
        public string ShardSetName { get; set; }
        public IList<Shard> Shards { get; set; }
        public int TargetShardCount { get; set; }
        public int Version { get; set; }

        #endregion

        #region constructors

        public ShardSetConfig()
        {
            ShardSetID = -1;
            ShardSetConfigID = -1;
            Version = -1;
            ShardMap = new ShardMap();
            Servers = new List<Server>();
            Shards = new List<Shard>();
            ShardSetConfigSettings = new List<ShardSetConfigSetting>();
        }

        #endregion

        #region events

        private event TableConfigPublishingHandler ShardSetConfigPublishing;

        #endregion

        #region methods

        /// <summary>
        /// Copies the shardlet using the shard set driver for the shard.
        /// </summary>
        /// <param name="sourceShard">The current shard.</param>
        /// <param name="destinationShard">The shard.</param>
        /// <param name="shardingKey">The sharding key.</param>
        /// <param name="uniqueProcessID">The unique process identifier.</param>
        public virtual void CopyShardlet(ShardBase sourceShard, ShardBase destinationShard, string shardingKey,
            Guid uniqueProcessID)
        {
            var driver = GetShardSetDriver();
            driver.CopyShardlet(sourceShard, destinationShard, this, shardingKey, uniqueProcessID);
        }

        /// <summary>
        /// Deletes the shardlet using the shard set driver for the shard.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardingKey">The sharding key.</param>
        public virtual void DeleteShardlet(ShardBase shard, string shardingKey)
        {
            var driver = GetShardSetDriver();
            driver.DeleteShardlet(shard, this, shardingKey);
        }

        /// <summary>
        /// Deploys the pointer shards for the shard set.
        /// </summary>
        /// <param name="queueRequest">if set to <c>true</c> queue request.</param>
        /// <exception cref="System.InvalidOperationException">Only the current ShardSetConfig can be used to publish a ShardMap</exception>
        public virtual void DeployPointerShards(bool queueRequest = false)
        {
            DeployShards(Shards, queueRequest);
        }

        /// <summary>
        /// Deploys the shard map for the shard set.
        /// </summary>
        /// <param name="queueRequest">if set to <c>true</c> queue request.</param>
        /// <exception cref="System.InvalidOperationException">Only the current ShardSetConfig can be used to publish a ShardMap</exception>
        public virtual void DeployShardMap(bool queueRequest = false)
        {
            DeployShards(ShardMap.Shards, queueRequest);
        }

        /// <summary>
        /// Gets the shard set setting value.
        /// </summary>
        /// <param name="settingKey">The setting key.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="KeyNotFoundException"> if the key does not exist</exception>
        public string GetShardSetSetting(string settingKey)
        {
            var setting =
                ShardSetConfigSettings
                    .FirstOrDefault(wgcs => wgcs.SettingKey == settingKey);

            if (setting == null)
                throw new KeyNotFoundException(string.Format("Missing setting '{0}' from work load group settings",
                    settingKey));

            return setting.SettingValue;
        }

        /// <summary>
        /// Gets the table list.
        /// </summary>
        /// <returns>IList{System.String}.</returns>
        public static IList<string> GetTableList()
        {
            return ScaleOutConfigManager.GetManager().GetTableList();
        }

        /// <summary>
        /// Handles the shard creation request and processes it against the shard set driver.
        /// </summary>
        /// <param name="request">The shard creation request.</param>
        /// <param name="queue">The queue.</param>
        public static void HandleShardCreationRequest(ShardCreationRequest request,
            IShardSetActionQueue queue)
        {
            var config = GetCurrentShardSetConfig(request.ShardSetName);
            var driver = GetShardSetDriver(config);

            Action<ShardCreationRequest> process = (x =>
            {
                var shard = new RangeShard
                {
                    Catalog = request.Catalog,
                    ServerInstanceName = request.ServerInstanceName,
                };

                driver.CreateShard(shard, config);
            });

            Action<ShardCreationRequest> complete =
                (x => queue.SendQueueProcessingEvent(GetCompletionMessage("ShardCreationRequest", x)));
            Action<ShardCreationRequest> error =
                (x => queue.SendQueueProcessingEvent(GetErrorMessage("ShardCreationRequest", x)));

            request.Process(process, complete, error);
        }

        /// <summary>
        /// Handles the shard deletion request and processes it against the shard set driver.
        /// </summary>
        /// <param name="request">The shard deletion request.</param>
        /// <param name="queue">The queue.</param>
        public static void HandleShardDeletionRequest(ShardDeletionRequest request,
            IShardSetActionQueue queue)
        {
            var config = GetCurrentShardSetConfig(request.ShardSetName);
            var driver = GetShardSetDriver(config);

            Action<ShardDeletionRequest> process = (x =>
            {
                var shard = new RangeShard
                {
                    Catalog = request.Catalog,
                    ServerInstanceName = request.ServerInstanceName,
                };
                //Do not delete the database if it is populated for now, as the move shardlet code could error leaving shardlets behind
                //TODO: Allow this to delete populated databases, by orchestrating that a failed shardlet move stops this step for that database.
                driver.DeleteShard(shard, config);
            });

            Action<ShardDeletionRequest> complete =
                (x => queue.SendQueueProcessingEvent(GetCompletionMessage("ShardDeletionRequest", x)));
            Action<ShardDeletionRequest> error =
                (x => queue.SendQueueProcessingEvent(GetErrorMessage("ShardDeletionRequest", x)));

            request.Process(process, complete, error);
        }

        /// <summary>
        /// Handles the shard map publishing request and processes it against the shard set driver.
        /// </summary>
        /// <param name="request">The shard map publishing request.</param>
        /// <param name="queue">The queue.</param>
        public static void HandleShardMapPublishingRequest(ShardMapPublishingRequest request,
            IShardSetActionQueue queue)
        {
            var shardSetConfig = GetCurrentShardSetConfig(request.ShardSetName);

            Action<ShardMapPublishingRequest> process =
                x =>
                {
                    ProcessShardMapPublishingRequest(x, shardSetConfig);

                    if (!request.ShouldUpdateShardMap) return;

                    PublishRangeMap(shardSetConfig.ShardSetName, request.CurrentShardMapID, request.NewShardMapID);
                };

            Action<ShardMapPublishingRequest> complete =
                x =>
                {
                    CompleteShardMapPublishingRequest(shardSetConfig, x);
                    queue.SendQueueProcessingEvent(GetCompletionMessage("ShardMapPublishingRequest", x));
                };

            Action<ShardMapPublishingRequest> error =
                x => queue.SendQueueProcessingEvent(GetErrorMessage("ShardMapPublishingRequest", x));

            request.Process(process, complete, error);
        }

        /// <summary>
        /// Handles the shard synchronize request and processes it against the shard set driver.
        /// </summary>
        /// <param name="request">The shard synchronize request.</param>
        /// <param name="queue">The queue.</param>
        public static void HandleShardSyncRequest(ShardSyncRequest request, BaseShardSetActionQueue queue)
        {
            var config = GetCurrentShardSetConfig(request.ShardSetName);
            var driver = GetShardSetDriver(config);

            Action<ShardSyncRequest> process = (
                x =>
                {
                    var shard = new RangeShard
                    {
                        Catalog = request.Catalog,
                        ServerInstanceName = request.ServerInstanceName,
                    };

                    driver.SyncShard(shard, config);
                });

            Action<ShardSyncRequest> complete =
                (x => queue.SendQueueProcessingEvent(GetCompletionMessage("ShardSyncRequest", x)));
            Action<ShardSyncRequest> error =
                (x => queue.SendQueueProcessingEvent(GetErrorMessage("ShardSyncRequest", x)));

            request.Process(process, complete, error);
        }

        /// <summary>
        /// Loads the current configuration for the shard set.
        /// </summary>
        /// <param name="shardSetName">Name of the table.</param>
        /// <returns>ShardSetConfig.</returns>
        public static ShardSetConfig LoadCurrent(string shardSetName)
        {
            return GetCurrentShardSetConfig(shardSetName);
        }

        /// <summary>
        /// Publishes the shard map for the shard set.
        /// </summary>
        /// <param name="queueRequest">if set to <c>true</c> queue request.</param>
        public virtual void PublishShardMap(bool queueRequest = false)
        {
            var previousShardMap = -1;
            //This could be null because it might be the first time the shard map is published.
            if (CurrentPublishedShardMapID.HasValue)
            {
                previousShardMap = CurrentPublishedShardMapID.Value;
            }

            var request =
                new ShardMapPublishingRequest
                {
                    NewShardMapID = ShardMap.ShardMapID,
                    ShardSetName = ShardSetName,
                    CurrentShardMapID = previousShardMap
                };

            if (queueRequest)
            {
                request.Save();
                return;
            }

            ProcessShardMapPublishingRequest(request, this);
        }

        /// <summary>
        /// Saves this instance to configuration.
        /// </summary>
        /// <returns>ShardSetConfig.</returns>
        public ShardSetConfig Save()
        {
            return ScaleOutConfigManager.GetManager().SaveConfiguration(this);
        }

        /// <summary>
        /// Sets the shard set setting value.
        /// </summary>
        /// <param name="settingKey">The setting key.</param>
        /// <param name="settingValue">The setting value.</param>
        public void SetShardSetSetting(string settingKey, string settingValue)
        {
            var setting =
                ShardSetConfigSettings
                    .FirstOrDefault(wgcs => wgcs.SettingKey == settingKey);

            if (setting == null)
            {
                setting =
                    new ShardSetConfigSetting
                    {
                        SettingKey = settingKey,
                        SettingValue = settingValue,
                    };

                ShardSetConfigSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = settingValue;
            }
        }


        /// <summary>
        /// Synchronizes the shards for the shard set.
        /// </summary>
        /// <param name="queueRequest">if set to <c>true</c> queue request.</param>
        public virtual void SyncPointerShards(bool queueRequest = false)
        {
            SyncShards(Shards, queueRequest);
        }

        /// <summary>
        /// Synchronizes the shards for the shard set.
        /// </summary>
        /// <param name="queueRequest">if set to <c>true</c> queue request.</param>
        public virtual void SyncShards(bool queueRequest = false)
        {
            SyncShards(ShardMap.Shards, queueRequest);
        }

        /// <summary>
        /// Updates the shard map.
        /// </summary>
        /// <returns>ShardSetConfig.</returns>
        public virtual ShardSetConfig UpdateShardMap()
        {
            var settings = Settings.Load();
            UpdateRangedBaseShardMap(settings);

            return this;
        }

        private static void CheckResultsAndThrow(ConcurrentBag<ShardDeploymentResult> parallelResults)
        {
            // todo: revisit how to throw these exceptions, log etc.
            if (parallelResults.Any(r => r.Exception != null))
            {
                throw new AggregateException(parallelResults.Select(r => r.Exception));
            }
        }

        private static void CompleteShardMapPublishingRequest(ShardSetConfig config,
            ShardMapPublishingRequest shardMapPublishingRequest)
        {
            //todo: this is saving a whole'nother config rathr thanjust incrementing the "current" config map in force
            config.CurrentPublishedShardMapID = shardMapPublishingRequest.NewShardMapID;
            config.Save();
        }

        private void DeployShards(IEnumerable<ShardBase> shards, bool queueRequest = false)
        {
            if (!IsCurrent)
            {
                throw new InvalidOperationException("Only the current ShardSetConfig can be used to publish Shards");
            }

            if (queueRequest)
            {
                foreach (var x in shards)
                {
                    var request =
                        new ShardCreationRequest
                        {
                            Catalog = x.Catalog,
                            ServerInstanceName = x.ServerInstanceName,
                            ShardSetName = ShardSetName,
                        };

                    request.Save();
                }

                return;
            }

            var driver = GetShardSetDriver();
            var parallelOptions = GetParallelOptions();
            var parallelResults = new ConcurrentBag<ShardDeploymentResult>();

            Parallel.ForEach(shards, parallelOptions, shard =>
            {
                Exception exception = null;

                try
                {
                    driver.CreateShard(shard, this);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                if (exception != null)
                {
                    parallelResults.Add(
                        new ShardDeploymentResult
                        {
                            Exception = exception,
                            Shard = shard
                        });
                }
            });

            CheckResultsAndThrow(parallelResults);
        }

        private static string GetCompletionMessage(string requestName, BaseQueueRequest request)
        {
            return string.Format("Processing complete for {0}({1})", requestName, request.QueueId);
        }

        private static ShardSetConfig GetCurrentShardSetConfig(string shardSetName)
        {
            return ScaleOutConfigManager.GetManager().GetCurrentShardSetConfig(shardSetName);
        }

        private static string GetErrorMessage(string requestName, BaseQueueRequest request)
        {
            return string.Format("Error {0}({1}):{2}", requestName, request.QueueId, request.Message);
        }

        private static ParallelOptions GetParallelOptions()
        {
            var publishingThreadCount = ConfigurationManager.AppSettings["PublishingThreads"];

            int threadCount;
            if (!int.TryParse(publishingThreadCount, out threadCount))
            {
                threadCount = 1;
            }
            else
            {
                // ignore invalid values
                if (threadCount < 0 || threadCount > 16) threadCount = 1;

                // if the value is set to 0, return new options with no MaxDegreeOfParallelism
                // which implies set the default number of threads based on the umber of CPUs.
                if (threadCount == 0)
                    return new ParallelOptions();
            }

            return new ParallelOptions {MaxDegreeOfParallelism = threadCount};
        }

        private static ShardMap GetShardMap(int shardMapId)
        {
            return ScaleOutConfigManager.GetManager().GetShardMap(shardMapId);
        }

        private static IShardSetConnectionDriver GetShardSetConnectionDriver(string shardSetName)
        {
            return ScaleOutShardletManager.GetManager().GetShardletConnectionDriver(shardSetName);
        }

        private IShardSetConnectionDriver GetShardSetConnectionDriver()
        {
            return GetShardSetConnectionDriver(ShardSetName);
        }

        private IShardSetDriver GetShardSetDriver()
        {
            return ScaleOutConfigManager.GetManager().GetShardSetDriver(this);
        }

        private static IShardSetDriver GetShardSetDriver(string shardSetName)
        {
            var config = GetCurrentShardSetConfig(shardSetName);

            return ScaleOutConfigManager.GetManager().GetShardSetDriver(config);
        }

        private static IShardSetDriver GetShardSetDriver(ShardSetConfig config)
        {
            return ScaleOutConfigManager.GetManager().GetShardSetDriver(config);
        }

        private static void ProcessShardMapPublishingRequest(ShardMapPublishingRequest shardMapPublishingRequest,
            ShardSetConfig shardSetConfig)
        {
            var shardSetName = shardMapPublishingRequest.ShardSetName;
            var shardSetConnectionDriver = GetShardSetConnectionDriver(shardSetName);

            // to do - may need to page
            var shardlets = shardSetConnectionDriver.GetShardlets(shardSetName);

            foreach (var shardlet in shardlets)
            {
                // skip if shardlet is pinned
                if (shardlet.Pinned) continue;

                // get the matching range shard for this shardlet based on the range map
                var rangeShard =
                    shardSetConfig.ShardMap.Shards
                        .FirstOrDefault(rs => rs.HighDistributionKey > shardlet.DistributionKey);

                // skip if we cannot find a range.  
                if (rangeShard == null) continue;

                // determine if the shardlet needs to be moved
                if (rangeShard.ServerInstanceName == shardlet.ServerInstanceName
                    && rangeShard.Catalog == shardlet.Catalog)
                {
                    continue;
                }

                var request =
                    new ShardletMoveRequest
                    {
                        DestinationCatalog = rangeShard.Catalog,
                        DestinationServerInstanceName = shardlet.ServerInstanceName,
                        SourceCatalog = shardlet.Catalog,
                        SourceServerInstanceName = shardlet.ServerInstanceName,
                        DistributionKey = shardlet.DistributionKey,
                        ShardingKey = shardlet.ShardingKey,
                        ShardSetName = shardlet.ShardSetName,
                        DeleteOnMove = true,
                        Pin = false
                    };

                request.Save();
            }

            // todo: do this save in parallel
            //var parallelOptions = GetParallelOptions();

            //Parallel.ForEach(shardletMoveRequests, parallelOptions,
            //    shardletMoveRequest => shardletMoveRequest.Save());
        }

        private static void PublishRangeMap(string shardSetName, int currentMapID, int newShardMapID)
        {
            var currentMap =
                currentMapID > 0
                    ? GetShardMap(currentMapID)
                    : null;

            var newShardMap = GetShardMap(newShardMapID);

            var shardSetConnectionDriver = GetShardSetConnectionDriver(shardSetName);
            var parallelOptions = GetParallelOptions();

            if (currentMap != null)
            {
                Parallel.ForEach(currentMap.Shards, parallelOptions,
                    currentShard => shardSetConnectionDriver.RemoveShard(shardSetName, currentShard));
            }

            Parallel.ForEach(newShardMap.Shards, parallelOptions,
                newShard => shardSetConnectionDriver.PublishShard(shardSetName, newShard));

            if (currentMap == null) return;

            //Get a list of all the published shards to be retired
            var retiredShards =
                currentMap.Shards
                    .Where(cs =>
                        !newShardMap.Shards.Any(
                            ns =>
                                ns.Catalog == cs.Catalog
                                && ns.ServerInstanceName == cs.ServerInstanceName));

            //Finally queue up to delete all of the retired shards.. This queue should only be processed once all of the shard moves have happened.
            Parallel.ForEach(retiredShards, parallelOptions, shard => (
                new ShardDeletionRequest
                {
                    Catalog = shard.Catalog,
                    ServerInstanceName = shard.ServerInstanceName,
                    ShardSetName = shardSetName
                }).Save());
        }

        private void SendShardSetPublishingMessage(string message)
        {
            if (ShardSetConfigPublishing != null)
            {
                ShardSetConfigPublishing(this, message);
            }
        }

        private void SyncShards(IEnumerable<ShardBase> shards, bool queueRequest = false)
        {
            if (queueRequest)
            {
                foreach (var shard in shards)
                {
                    var request = new ShardSyncRequest
                    {
                        Catalog = shard.Catalog,
                        ServerInstanceName = shard.ServerInstanceName,
                        ShardSetName = ShardSetName,
                    };

                    request.Save();
                }

                return;
            }

            var driver = GetShardSetDriver();

            var parallelOptions = GetParallelOptions();
            var parallelResults = new ConcurrentBag<ShardDeploymentResult>();
            Parallel.ForEach(shards, parallelOptions, shard =>
            {
                Exception ex = null;
                try
                {
                    driver.SyncShard(shard, this);
                }
                catch (Exception e)
                {
                    ex = e;
                }
                if (ex != null)
                {
                    parallelResults.Add(new ShardDeploymentResult
                    {
                        Exception = ex,
                        Shard = shard
                    });
                }
            });

            CheckResultsAndThrow(parallelResults);
        }

        private void UpdateRangedBaseShardMap(Settings settings)
        {
            //Rollback available shards on each server for the recalculation
            foreach (var server in Servers)
            {
                if (server.MaxShardsAllowed > 0)
                {
                    //TODO: in order for this to work "right" we need an accurate calculation of available shards from the DAL.. Until then we will pretend that setting is max shards per shard set.
                    server.AvailableShards = server.MaxShardsAllowed;
                    //server.AvailableShards + ShardMap.Shards.Count(x => x.ServerInstanceName == server.ServerInstanceName);
                }
            }
            var newShardCount = TargetShardCount - ShardMap.Shards.Count();
            IList<RangeShard> shardsUsed = new List<RangeShard>();
            if (newShardCount > 0)
            {
                //If Max Shards < 1 then unlimited shards allowed on the server.
                var availableShards = Servers.Sum(x => (x.MaxShardsAllowed < 1 ? 1000000 : x.AvailableShards));
                if (availableShards < newShardCount)
                {
                    throw new Exception("Not enough Shards available on servers used in shard set");
                }
            }
            var increment = (long.MaxValue/TargetShardCount)*2;
            var rangeLowValue = Int64.MinValue;
            var rangeHighValue = Int64.MaxValue;
            if (increment != -2)
            {
                rangeHighValue = rangeLowValue + increment;
            }
            var counter = 0;
            var currServerIndex = 0;

            var currentServer = Servers[currServerIndex];
            while (counter < TargetShardCount)
            {
                if ((currentServer.AvailableShards <= 0)
                    && (currentServer.MaxShardsAllowed > 0))
                    //unlimited shards allowed maxshardsallowed < 1 (or in this case > 0)
                {
                    currServerIndex++;
                    currentServer = Servers[currServerIndex];
                }
                else
                {
                    if (currentServer.MaxShardsAllowed > 0)
                    {
                        currentServer.AvailableShards--;
                    }
                }
                counter++;
                var databaseName = settings.ShardPrefix + ShardSetName + (counter.ToString("000000"));
                var isNewShard = false;
                var shard = ShardMap.Shards.FirstOrDefault(x => x.Catalog == databaseName);
                if (shard == null)
                {
                    shard = new RangeShard();
                    ShardMap.ShardMapID = -1;
                    isNewShard = true;
                }

                //Is anything changing?
                if (
                    shard.Catalog != databaseName ||
                    shard.LowDistributionKey != rangeLowValue ||
                    shard.HighDistributionKey != rangeHighValue ||
                    shard.ServerInstanceName != currentServer.ServerInstanceName
                    )
                {
                    shard.Catalog = databaseName;
                    shard.LowDistributionKey = rangeLowValue;
                    shard.HighDistributionKey = rangeHighValue;
                    shard.ServerInstanceName = currentServer.ServerInstanceName;
                    //If the shard has changed then time for a new map.
                    ShardMap.ShardMapID = -1;
                    shard.ShardID = -1;
                }

                //Only add the shards that are new.
                if (isNewShard)
                {
                    ShardMap.Shards.Add(shard);
                }
                shardsUsed.Add(shard);

                //Either there can be a near infinite number of shards in the server, or we need to calculate how many shards remaining for the shard set
                //TODO: don't query just the current shard set, figure out total number of used shards across all work groups.
                //TODO: don't query just the current shard set, figure out total number of used shards across all work groups.
                currentServer.AvailableShards = (currentServer.MaxShardsAllowed == -1
                    ? 100000
                    : (currentServer.MaxShardsAllowed -
                       shardsUsed.Count(x => x.ServerInstanceName == currentServer.ServerInstanceName)));

                rangeLowValue = rangeHighValue + 1;
                if (counter + 1 == TargetShardCount)
                {
                    rangeHighValue = Int64.MaxValue;
                }
                else
                {
                    rangeHighValue += increment;
                }
            }
            var shardsToDelete = ShardMap.Shards.Count(x => !shardsUsed.Contains(x));
            if (shardsToDelete > 0)
            {
                //If we are deleting shards, then its time for a new shard map.
                ShardMap.ShardMapID = -1;
            }

            ShardMap.Shards = shardsUsed;

            // if the shard map is new all of the shards must be new too... 
            if (ShardMap.ShardMapID == -1)
            {
                ShardMap.Shards.ToList().ForEach(x => x.ShardID = -1);
            }
        }

        #endregion
    }
}