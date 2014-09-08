#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    /// <summary>
    /// Class Shardlet represents one set of data from a ShardSet.
    /// </summary>
    public class Shardlet : ShardConnection
    {
        #region properties

        /// <summary>
        /// Gets or sets the distribution key.  This is the key used to identity the Shard
        /// the Shardlet is mapped to.
        /// </summary>
        /// <value>The distribution key.</value>
        public long DistributionKey { get; set; }

        /// <summary>
        /// Pinned tells Data Elasticity that the shard information did not come from the map.  
        /// The IShardSetConnectionDriver is responsible for setting this value!
        /// </summary>
        public bool Pinned { get; set; }

        /// <summary>
        /// Gets or sets the sharding key.
        /// </summary>
        /// <value>The sharding key.</value>
        public string ShardingKey { get; set; }

        /// <summary>
        /// Gets or sets the status of the ShardLet.
        /// </summary>
        /// <value>The status.</value>
        public ShardletStatus Status { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Handles the shardlet move request from tje request queue.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="queue">The queue.</param>
        /// <param name="uniqueProcessID">A unique process identifier that can be set to maintain state between moves.</param>
        public static void HandleShardletMoveRequest(ShardletMoveRequest request, IShardSetActionQueue queue,
            Guid uniqueProcessID)
        {
            Action<ShardletMoveRequest> process =
                x =>
                {
                    var shardlet = Load(x.ShardSetName, x.ShardingKey);
                    var destinationShard =
                        new RangeShard
                        {
                            Catalog = x.DestinationCatalog,
                            ServerInstanceName = x.DestinationServerInstanceName
                        };

                    shardlet.MoveToShard(destinationShard, x.Pin, x.DeleteOnMove, uniqueProcessID);
                };

            Action<ShardletMoveRequest> complete =
                x =>
                {
                    var message = GetCompletionMessage("ShardletMoveRequest", x);
                    queue.SendQueueProcessingEvent(message);
                };

            Action<ShardletMoveRequest> error =
                x =>
                {
                    var message = GetErrorMessage("ShardletMoveRequest", x);
                    queue.SendQueueProcessingEvent(message);
                };

            request.Process(process, complete, error);
        }

        /// <summary>
        /// Loads the specified shard set by name and sharding key.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="shardingKey">The sharding key to distribute.</param>
        /// <param name="spid">The spid of the connecting process.</param>
        /// <returns>Shardlet.</returns>
        public static Shardlet Load(string shardSetName, string shardingKey, short? spid = null)
        {
            return ScaleOutShardletManager.GetManager().GetShardlet(shardSetName, shardingKey, spid);
        }

        /// <summary>
        /// Connects the specified shardlet.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <param name="spid">The spid.</param>
        public static void Connect(Shardlet shardlet, short spid)
        {
            ScaleOutShardletManager.GetManager().Connect(shardlet, spid);
        }

        /// <summary>
        /// Disconnects the specified shardlet.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <param name="spid">The spid.</param>
        public static void Disconnect(Shardlet shardlet, short spid)
        {
            ScaleOutShardletManager.GetManager().Disconnect(shardlet, spid);
        }

        /// <summary>
        /// Loads the specified shard set by name and distribution key.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="distributionKey">The distribution key.</param>
        /// <param name="isNew">if set to <c>true</c> [is new].</param>
        /// <returns>Shardlet.</returns>
        /// <remarks>
        /// If the isNew flag is set, the Load method will not look in the pinned shard map for the existence
        /// of the ShardLet.  
        /// </remarks>
        public static Shardlet Load(string shardSetName, long distributionKey, bool isNew = false)
        {
            return ScaleOutShardletManager.GetManager().GetShardlet(shardSetName, distributionKey, isNew);
        }

        /// <summary>
        /// Loads the specified shard set by name and distribution key.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="dataSetName">Name of the data set.</param>
        /// <param name="uniqueValue">The unique value.</param>
        /// <returns>Shardlet.</returns>
        public static Shardlet Load(string shardSetName, string dataSetName, string uniqueValue)
        {
            return ScaleOutShardletManager.GetManager().GetShardlet(shardSetName, dataSetName, uniqueValue);
        }

        /// <summary>
        /// Moves to shard.
        /// </summary>
        /// <param name="shard">The shard to move the ShardLet data to.</param>
        /// <param name="pin">if set to <c>true</c> add the shard to the pinned shard list.</param>
        /// <param name="deleteOnMove">if set to <c>true</c> delete the Shardlet data from the existing Shard after move.</param>
        /// <param name="uniqueProcessID">A unique process identifier that can be set to maintain state between moves.</param>
        /// <param name="queueRequest">if set to <c>true</c> queue the shard move rather than executing it immediately.</param>
        public void MoveToShard(ShardBase shard, bool pin, bool deleteOnMove, Guid uniqueProcessID,
            bool queueRequest = false)
        {
            if (queueRequest)
            {
                var request =
                    new ShardletMoveRequest
                    {
                        DestinationCatalog = shard.Catalog,
                        DestinationServerInstanceName = shard.ServerInstanceName,
                        SourceCatalog = Catalog,
                        SourceServerInstanceName = ServerInstanceName,
                        DistributionKey = DistributionKey,
                        ShardingKey = ShardingKey,
                        ShardSetName = ShardSetName,
                        DeleteOnMove = deleteOnMove,
                        UniqueProcessID = uniqueProcessID,
                        Pin = pin
                    };

                request.Save();

                return;
            }

            var shardSetConnectionDriver = ScaleOutShardletManager.GetManager().GetShardletConnectionDriver(ShardSetName);
            try
            {
                // change the state to read only in the map and update th epin status
                Status = ShardletStatus.Moving;               
                Pinned = pin;
                shardSetConnectionDriver.PublishShardlet(this);

                // terminate all existing connections
                shardSetConnectionDriver.TerminateConnections(this);

                // copy the shardlet data
                var shardSetConfig = ShardSetConfig.LoadCurrent(ShardSetName);

                var currentShard =
                    new RangeShard
                    {
                        Catalog = Catalog,
                        ServerInstanceName = ServerInstanceName,
                    };

                shardSetConfig.CopyShardlet(currentShard, shard, ShardingKey, uniqueProcessID);

                // update the status and set the new catalog and instance in the shard map
                Catalog = shard.Catalog;
                ServerInstanceName = shard.ServerInstanceName;
                Status = ShardletStatus.Active;

                shardSetConnectionDriver.PublishShardlet(this);

                if (deleteOnMove)
                {
                    shardSetConfig.DeleteShardlet(currentShard, ShardingKey);
                }
            }
            catch (Exception)
            {
                Status = ShardletStatus.Active;
                shardSetConnectionDriver.PublishShardlet(this);

                throw;
            }
        }

        private static string GetCompletionMessage(string requestName, BaseQueueRequest request)
        {
            return string.Format("Processing complete for {0}({1})", requestName, request.QueueId);
        }

        private static string GetErrorMessage(string requestName, BaseQueueRequest request)
        {
            return string.Format("Error {0}({1}):{2}", requestName, request.QueueId, request.Message);
        }

        #endregion
    }
}