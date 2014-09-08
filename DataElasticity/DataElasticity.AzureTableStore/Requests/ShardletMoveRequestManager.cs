#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Queues;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Requests
{
    internal class ShardletMoveRequestManager :
        RequestManagerBase<AzureShardletMove, ShardletMoveRequest>
    {
        #region properties

        protected override string TableName
        {
            get { return Constants.ShardletMovesTable; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShardletMoveRequest" /> class.
        /// </summary>
        /// <param name="queue">The action queue holding the Data Elasticity actions.</param>
        /// <param name="tableClient">The table client.</param>
        public ShardletMoveRequestManager(CloudQueue queue, CloudTableClient tableClient)
            : base(queue, tableClient)
        {
        }

        #endregion

        #region methods

        /// <summary>
        /// Saves the specified shard creation request.
        /// </summary>
        /// <param name="request">The shardlet move request.</param>
        /// <returns>ShardletMoveRequest.</returns>
        /// <exception cref="System.Exception">Entity not found by QueryId</exception>
        public ShardletMoveRequest Save(ShardletMoveRequest request)
        {
            var shardletMovesTable = TableClient.GetTableReference(TableName);

            //-1 means its new... 
            if (request.QueueId == -1)
            {
                var rowKey = DateTime.Now.Ticks;
                var azureShardletMove =
                    new AzureShardletMove
                    {
                        DeleteOnMove = request.DeleteOnMove,
                        SourceCatalog = request.SourceCatalog,
                        SourceServerInstanceName = request.SourceServerInstanceName,
                        DestinationCatalog = request.DestinationCatalog,
                        DestinationServerInstanceName = request.DestinationServerInstanceName,
                        ShardSetName = request.ShardSetName,
                        LongRowKey = rowKey,
                        Status = TableActionQueueItemStatus.Queued.ToString(),
                        Pin = request.Pin,
                        LastTouched = DateTime.UtcNow,
                        Message = request.Message,
                        DistributionKey = request.DistributionKey,
                        ShardingKey = request.ShardingKey,
                    };

                request.QueueId = rowKey;
                request.LastTouched = azureShardletMove.LastTouched;

                InsertTableAction(shardletMovesTable, azureShardletMove);
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

                action.DeleteOnMove = request.DeleteOnMove;
                action.SourceCatalog = request.SourceCatalog;
                action.SourceServerInstanceName = request.SourceServerInstanceName;
                action.DestinationCatalog = request.DestinationCatalog;
                action.DestinationServerInstanceName = request.DestinationServerInstanceName;
                action.ShardSetName = request.ShardSetName;
                action.LongRowKey = request.QueueId;
                action.Status = request.Status.ToString();
                action.Pin = request.Pin;
                action.LastTouched = DateTime.UtcNow;
                action.DistributionKey = request.DistributionKey;
                action.ShardingKey = request.ShardingKey;
                action.Message = request.Message;
                request.LastTouched = action.LastTouched;

                MergeTableAction(shardletMovesTable, action);
            }

            return request;
        }

        /// <summary>
        /// Creates the shard action request.
        /// </summary>
        /// <param name="action">The action the request is created from.</param>
        /// <returns>ShardletMoveRequest.</returns>
        protected override ShardletMoveRequest CreateRequest(AzureShardletMove action)
        {
            return new ShardletMoveRequest
            {
                DeleteOnMove = action.DeleteOnMove,
                DestinationCatalog = action.DestinationCatalog,
                DestinationServerInstanceName = action.DestinationServerInstanceName,
                LastTouched = action.LastTouched,
                Message = action.Message,
                Pin = action.Pin,
                QueueId = action.LongRowKey,
                SourceCatalog = action.SourceCatalog,
                SourceServerInstanceName = action.SourceServerInstanceName,
                Status =
                    (TableActionQueueItemStatus)
                        Enum.Parse(typeof (TableActionQueueItemStatus), action.Status),
                DistributionKey = action.DistributionKey,
                ShardingKey = action.ShardingKey,
                ShardSetName = action.ShardSetName
            };
        }

        #endregion
    }
}