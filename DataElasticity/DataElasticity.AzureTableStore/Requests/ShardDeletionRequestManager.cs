#region usings

using System;
using System.Globalization;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Queues;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Requests
{
    internal class ShardDeletionRequestManager :
        RequestManagerBase<AzureShardAction, ShardDeletionRequest>
    {
        #region properties

        protected override string TableName
        {
            get { return Constants.ShardDeletionsTable; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShardDeletionRequestManager" /> class.
        /// </summary>
        /// <param name="queue">The action queue holding the Data Elasticity actions.</param>
        /// <param name="tableClient">The table client.</param>
        public ShardDeletionRequestManager(CloudQueue queue, CloudTableClient tableClient)
            : base(queue, tableClient)
        {
        }

        #endregion

        #region methods

        /// <summary>
        /// Saves the specified shard creation request.
        /// </summary>
        /// <param name="request">The shard creation request.</param>
        /// <returns>ShardDeletionRequest.</returns>
        /// <exception cref="System.Exception">Entity not found by QueryId</exception>
        public ShardDeletionRequest Save(ShardDeletionRequest request)
        {
            var shardDeletionTable = TableClient.GetTableReference(TableName);

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

                InsertTableAction(shardDeletionTable, action);
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

                MergeTableAction(shardDeletionTable, action);
            }

            return request;
        }

        /// <summary>
        /// Creates the shard action request.
        /// </summary>
        /// <param name="action">The action the request is created from.</param>
        /// <returns>ShardDeletionRequest.</returns>
        protected override ShardDeletionRequest CreateRequest(AzureShardAction action)
        {
            return
                new ShardDeletionRequest
                {
                    Catalog = action.Catalog,
                    LastTouched = action.LastTouched,
                    QueueId = action.LongRowKey,
                    Message = action.Message,
                    ServerInstanceName = action.ServerInstanceName,
                    Status =
                        (TableActionQueueItemStatus)
                            Enum.Parse(typeof (TableActionQueueItemStatus), action.Status),
                    ShardSetName = action.ShardSetName,
                };
        }

        #endregion
    }
}