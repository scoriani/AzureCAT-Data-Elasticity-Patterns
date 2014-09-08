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
    internal class ShardCreationRequestManager :
        RequestManagerBase<AzureShardAction, ShardCreationRequest>
    {
        #region properties

        protected override string TableName
        {
            get { return Constants.ShardCreationsTable; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShardCreationRequestManager" /> class.
        /// </summary>
        /// <param name="queue">The action queue holding the Data Elasticity actions.</param>
        /// <param name="tableClient">The table client.</param>
        public ShardCreationRequestManager(CloudQueue queue, CloudTableClient tableClient)
            : base(queue, tableClient)
        {
        }

        #endregion

        #region methods

        /// <summary>
        /// Saves the specified shard creation request.
        /// </summary>
        /// <param name="request">The shard creation request.</param>
        /// <returns>ShardCreationRequest.</returns>
        /// <exception cref="System.Exception">Entity not found by QueryId</exception>
        public ShardCreationRequest Save(ShardCreationRequest request)
        {
            var shardCreationsTable = TableClient.GetTableReference(TableName);

            //-1 means its new... 
            if (request.QueueId == -1)
            {
                var rowkey = DateTime.Now.Ticks;
                var action =
                    new AzureShardAction
                    {
                        Catalog = request.Catalog,
                        Message = request.Message,
                        ServerInstanceName = request.ServerInstanceName,
                        ShardSetName = request.ShardSetName,
                        LongRowKey = rowkey,
                        Status = TableActionQueueItemStatus.Queued.ToString(),
                        LastTouched = DateTime.UtcNow
                    };

                request.Status = TableActionQueueItemStatus.Queued;
                request.QueueId = rowkey;
                request.LastTouched = action.LastTouched;

                InsertTableAction(shardCreationsTable, action);
                AddQueueMessage(rowkey);
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

                MergeTableAction(shardCreationsTable, action);
            }

            return request;
        }

        /// <summary>
        /// Creates the shard action request.
        /// </summary>
        /// <param name="action">The action the request is created from.</param>
        /// <returns>ShardCreationRequest.</returns>
        protected override ShardCreationRequest CreateRequest(AzureShardAction action)
        {
            return
                new ShardCreationRequest
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