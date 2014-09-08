#region usings

using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Requests;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore
{
    // ReSharper disable once RedundantExtendsListEntry
    internal class AzureShardSetActionQueue : BaseShardSetActionQueue, IShardSetActionQueue
    {
        #region fields

        private readonly CloudTableClient _tableClient;

        private CloudQueue _shardCreationsQueue;
        private CloudQueue _shardDeletionsQueue;
        private CloudQueue _shardMapPublishingsQueue;
        private CloudQueue _shardSyncsQueue;
        private CloudQueue _shardletMoveQueue;

        #endregion

        #region constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AzureShardSetActionQueue" /> class.
        /// </summary>
        public AzureShardSetActionQueue()
        {
            var storageAccount = GetCloudStorageAccount();
            _tableClient = storageAccount.CreateCloudTableClient();

            CreateOrValidateTables();
            CreateOrValidateQueues(storageAccount);
        }

        #endregion

        #region IShardSetActionQueue

        /// <summary>
        ///     Gets the queued requests by status.
        /// </summary>
        /// <typeparam name="TRequest">The type of the t request.</typeparam>
        /// <param name="status">The status.</param>
        /// <returns>IList{TRequest}.</returns>
        /// <exception cref="System.Exception"></exception>
        public override IList<TRequest> GetQueuedRequestsByStatus<TRequest>(TableActionQueueItemStatus status)
        {
            switch (typeof (TRequest).Name)
            {
                case "ShardletMoveRequest":
                {
                    var action = GetShardletMoveRequestManager();
                    return action.GetActions(status) as IList<TRequest>;
                }
                case "ShardCreationRequest":
                {
                    var action = GetShardCreationRequestManager();
                    return action.GetActions(status) as IList<TRequest>;
                }
                case "ShardSyncRequest":
                {
                    var action = GetShardSyncRequestManager();
                    return action.GetActions(status) as IList<TRequest>;
                }
                case "ShardDeletionRequest":
                {
                    var action = GetShardDeletionRequestManager();
                    return action.GetActions(status) as IList<TRequest>;
                }
                case "ShardMapPublishingRequest":
                {
                    var action = GetShardMapPublishingRequestManager();
                    return action.GetActions(status) as IList<TRequest>;
                }
                default:
                {
                    throw new Exception(typeof (TRequest).Name + " Type is not supported by this queue");
                }
            }
        }

        /// <summary>
        ///     Saves the request to queue.
        /// </summary>
        /// <typeparam name="TRequest">The type of requests to save</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>A request of type TRequest</returns>
        public override TRequest SaveRequestToQueue<TRequest>(TRequest request)
        {
            switch (typeof (TRequest).Name)
            {
                case "ShardletMoveRequest":
                {
                    var action = GetShardletMoveRequestManager();
                    return action.Save(request as ShardletMoveRequest) as TRequest;
                }
                case "ShardCreationRequest":
                {
                    var action = GetShardCreationRequestManager();
                    return action.Save(request as ShardCreationRequest) as TRequest;
                }
                case "ShardSyncRequest":
                {
                    var action = GetShardSyncRequestManager();
                    return action.Save(request as ShardSyncRequest) as TRequest;
                }
                case "ShardDeletionRequest":
                {
                    var action = GetShardDeletionRequestManager();
                    return action.Save(request as ShardDeletionRequest) as TRequest;
                }
                case "ShardMapPublishingRequest":
                {
                    var action = GetShardMapPublishingRequestManager();
                    return action.Save(request as ShardMapPublishingRequest) as TRequest;
                }
                default:
                {
                    throw new Exception(typeof (TRequest).Name + " Type is not supported by this queue");
                }
            }
        }

        #endregion

        #region methods

        /// <summary>
        ///     Are any requests of type TRequest in process.
        /// </summary>
        /// <typeparam name="TRequest">The type of requests to return</typeparam>
        /// <returns><c>true</c> if there are requests of type TRequest in progress, <c>false</c> otherwise.</returns>
        protected override bool AreRequestsInProcess<TRequest>()
        {
            switch (typeof (TRequest).Name)
            {
                case "ShardletMoveRequest":
                {
                    var action = GetShardletMoveRequestManager();
                    return action.IsInProcess();
                }
                case "ShardCreationRequest":
                {
                    var action = GetShardCreationRequestManager();
                    return action.IsInProcess();
                }
                case "ShardSyncRequest":
                {
                    var action = GetShardSyncRequestManager();
                    return action.IsInProcess();
                }
                case "ShardDeletionRequest":
                {
                    var action = GetShardDeletionRequestManager();
                    return action.IsInProcess();
                }
                case "ShardMapPublishingRequest":
                {
                    var action = GetShardMapPublishingRequestManager();
                    return action.IsInProcess();
                }
                default:
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     Gets the next queued request.
        /// </summary>
        /// <typeparam name="TRequest">The type of the t request.</typeparam>
        /// <returns>A request of type T</returns>
        /// <exception cref="System.Exception"></exception>
        protected override TRequest GetNextQueuedRequest<TRequest>()
        {
            switch (typeof (TRequest).Name)
            {
                case "ShardletMoveRequest":
                {
                    var manager = GetShardletMoveRequestManager();
                    return manager.GetNextAction() as TRequest;
                }
                case "ShardCreationRequest":
                {
                    var manager = GetShardCreationRequestManager();
                    return manager.GetNextAction() as TRequest;
                }
                case "ShardSyncRequest":
                {
                    var manager = GetShardSyncRequestManager();
                    return manager.GetNextAction() as TRequest;
                }
                case "ShardDeletionRequest":
                {
                    var manager = GetShardDeletionRequestManager();
                    return manager.GetNextAction() as TRequest;
                }
                case "ShardMapPublishingRequest":
                {
                    var manager = GetShardMapPublishingRequestManager();
                    return manager.GetNextAction() as TRequest;
                }
                default:
                {
                    throw new Exception(typeof (TRequest).Name + " Type is not supported by this queue");
                }
            }
        }

        private void CreateOrValidateQueues(CloudStorageAccount storageAccount)
        {
            var queueClient = storageAccount.CreateCloudQueueClient();

            _shardletMoveQueue = queueClient.GetQueueReference(Constants.ShardletMovesQueue);
            _shardCreationsQueue = queueClient.GetQueueReference(Constants.ShardCreationsQueue);
            _shardSyncsQueue = queueClient.GetQueueReference(Constants.ShardSynchronizationsQueue);
            _shardDeletionsQueue = queueClient.GetQueueReference(Constants.ShardDeletionsQueue);
            _shardMapPublishingsQueue =
                queueClient.GetQueueReference(Constants.ShardMapPublishingsQueue);

            _shardletMoveQueue.CreateIfNotExists();
            _shardCreationsQueue.CreateIfNotExists();
            _shardSyncsQueue.CreateIfNotExists();
            _shardDeletionsQueue.CreateIfNotExists();
            _shardMapPublishingsQueue.CreateIfNotExists();
        }

        private void CreateOrValidateTables()
        {
            var shardletMoveTable = _tableClient.GetTableReference(Constants.ShardletMovesTable);
            var shardCreationsTable =
                _tableClient.GetTableReference(Constants.ShardCreationsTable);
            var shardSyncsTable = _tableClient.GetTableReference(Constants.ShardSyncsTable);
            var shardDeletionsTable =
                _tableClient.GetTableReference(Constants.ShardDeletionsTable);
            var shardMapPublishingsTable =
                _tableClient.GetTableReference(Constants.ShardMapPublishingsTable);

            shardletMoveTable.CreateIfNotExists();
            shardCreationsTable.CreateIfNotExists();
            shardSyncsTable.CreateIfNotExists();
            shardDeletionsTable.CreateIfNotExists();
            shardMapPublishingsTable.CreateIfNotExists();
        }

        private static CloudStorageAccount GetCloudStorageAccount()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["AzureStorage"];
            if (connectionString == null)
            {
                throw new Exception("Connection string to azure storage is required in your app.config");
            }

            var storageAccount = CloudStorageAccount.Parse(connectionString.ConnectionString);
            return storageAccount;
        }

        private ShardCreationRequestManager GetShardCreationRequestManager()
        {
            return new ShardCreationRequestManager(_shardCreationsQueue, _tableClient);
        }

        private ShardDeletionRequestManager GetShardDeletionRequestManager()
        {
            return new ShardDeletionRequestManager(_shardDeletionsQueue, _tableClient);
        }

        private ShardMapPublishingRequestManager GetShardMapPublishingRequestManager()
        {
            return new ShardMapPublishingRequestManager(_shardMapPublishingsQueue, _tableClient);
        }

        private ShardSyncRequestManager GetShardSyncRequestManager()
        {
            return new ShardSyncRequestManager(_shardSyncsQueue, _tableClient);
        }

        private ShardletMoveRequestManager GetShardletMoveRequestManager()
        {
            return new ShardletMoveRequestManager(_shardletMoveQueue, _tableClient);
        }

        #endregion
    }
}