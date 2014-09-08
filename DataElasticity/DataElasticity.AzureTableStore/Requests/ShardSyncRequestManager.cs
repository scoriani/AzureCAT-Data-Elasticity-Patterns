#region usings

using System;
using System.Globalization;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Queues;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Requests
{
    internal class ShardSyncRequestManager :
        RequestManagerBase<AzureShardAction, ShardSyncRequest>
    {
        #region properties

        protected override string TableName
        {
            get { return Constants.ShardSyncsTable; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShardSyncRequestManager" /> class.
        /// </summary>
        /// <param name="queue">The action queue holding the Data Elasticity actions.</param>
        /// <param name="tableClient">The table client.</param>
        public ShardSyncRequestManager(CloudQueue queue, CloudTableClient tableClient)
            : base(queue, tableClient)
        {
        }

        #endregion

        #region methods

        /// <summary>
        /// Saves the specified shard sync request.
        /// </summary>
        /// <param name="request">The shard synchronize request.</param>
        /// <returns>ShardSyncRequest.</returns>
        /// <exception cref="System.Exception">Entity not found by QueryId</exception>
        public ShardSyncRequest Save(ShardSyncRequest request)
        {
            var shardSyncsTable = TableClient.GetTableReference(TableName);
            //-1 means its new... 
            if (request.QueueId == -1)
            {
                var rowKey = DateTime.Now.Ticks;
                var action =
                    new AzureShardAction
                    {
                        Catalog = request.Catalog,
                        Message = request.Message,
                        ServerInstanceName = request.ServerInstanceName,
                        ShardSetName = request.ShardSetName,
                        LongRowKey = rowKey,
                        Status = TableActionQueueItemStatus.Queued.ToString(),
                        LastTouched = DateTime.UtcNow
                    };

                request.Status = TableActionQueueItemStatus.Queued;
                request.QueueId = rowKey;
                request.LastTouched = action.LastTouched;

                InsertTableAction(shardSyncsTable, action);
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

                action.Catalog = request.Catalog;
                action.Message = request.Message;
                action.ServerInstanceName = request.ServerInstanceName;
                action.ShardSetName = request.ShardSetName;
                action.Status = request.Status.ToString();
                action.LastTouched = DateTime.UtcNow;
                request.LastTouched = action.LastTouched;

                MergeTableAction(shardSyncsTable, action);
            }

            return request;
        }

        /// <summary>
        /// Creates the shard action request.
        /// </summary>
        /// <param name="action">The action the request is created from.</param>
        /// <returns>ShardSyncRequest.</returns>
        protected override ShardSyncRequest CreateRequest(AzureShardAction action)
        {
            return
                new ShardSyncRequest
                {
                    Catalog = action.Catalog,
                    LastTouched = action.LastTouched,
                    QueueId = action.LongRowKey,
                    Message = action.Message,
                    ServerInstanceName = action.ServerInstanceName,
                    Status =
                        (TableActionQueueItemStatus)
                            Enum.Parse(typeof (TableActionQueueItemStatus), action.Status),
                    ShardSetName = action.ShardSetName
                };
        }

        #endregion
    }
}