#region usings

using System;
using System.Globalization;
using System.Linq;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Queues;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Requests
{
    internal class ShardMapPublishingRequestManager :
        RequestManagerBase<ShardMapPublishing, ShardMapPublishingRequest>
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShardMapPublishing" /> class.
        /// </summary>
        /// <param name="queue">The action queue holding the Data Elasticity actions.</param>
        /// <param name="tableClient">The table client.</param>
        public ShardMapPublishingRequestManager(CloudQueue queue, CloudTableClient tableClient)
            : base(queue, tableClient)
        {
        }

        #endregion

        #region methods

        protected override string TableName
        {
            get { return Constants.ShardMapPublishingsTable; }
        }

        /// <summary>
        /// Saves the specified shard creation request.
        /// </summary>
        /// <param name="request">The shardlet move request.</param>
        /// <returns>ShardMapPublishingRequest.</returns>
        /// <exception cref="System.Exception">Entity not found by QueryId</exception>
        public ShardMapPublishingRequest Save(ShardMapPublishingRequest request)
        {
            var shardMapPublishingsTable = TableClient.GetTableReference(TableName);

            //-1 means its new... 
            if (request.QueueId == -1)
            {
                var rowKey = DateTime.Now.Ticks;
                var shardMapPublishing =
                    new ShardMapPublishing
                    {
                        NewShardMapID = request.NewShardMapID,
                        CurrentShardMapID = request.CurrentShardMapID,
                        Message = request.Message,
                        ShardSetName = request.ShardSetName,
                        LongRowKey = rowKey,
                        Status = TableActionQueueItemStatus.Queued.ToString(),
                        LastTouched = DateTime.UtcNow
                    };

                request.Status = TableActionQueueItemStatus.Queued;
                request.QueueId = rowKey;
                request.LastTouched = shardMapPublishing.LastTouched;

                InsertTableAction(shardMapPublishingsTable, shardMapPublishing);
                AddQueueMessage(rowKey);
            }
            else
            {
                var action = GetAction(request.QueueId);
                if (action == null)
                {
                    //todo: log?
                    throw new InvalidOperationException("Entity not found by QueryId");
                }

                action.NewShardMapID = request.NewShardMapID;
                action.CurrentShardMapID = request.CurrentShardMapID;
                action.Message = request.Message;
                action.ShardSetName = request.ShardSetName;
                action.Status = request.Status.ToString();
                action.LastTouched = DateTime.UtcNow;

                request.LastTouched = action.LastTouched;

                MergeTableAction(shardMapPublishingsTable, action);
            }

            return request;
        }

        /// <summary>
        /// Creates the shard action request.
        /// </summary>
        /// <param name="action">The action the request is created from.</param>
        /// <returns>ShardMapPublishingRequest.</returns>
        protected override ShardMapPublishingRequest CreateRequest(ShardMapPublishing action)
        {
            return
                new ShardMapPublishingRequest
                {
                    NewShardMapID = action.NewShardMapID,
                    CurrentShardMapID = action.CurrentShardMapID,
                    LastTouched = action.LastTouched,
                    QueueId = action.LongRowKey,
                    Message = action.Message,
                    Status =
                        (TableActionQueueItemStatus)
                            Enum.Parse(typeof (TableActionQueueItemStatus), action.Status),
                    ShardSetName = action.ShardSetName
                };
        }

        #endregion
    }
}