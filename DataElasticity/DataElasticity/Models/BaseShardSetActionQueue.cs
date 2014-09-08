#region usings

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;
using Microsoft.WindowsAzure;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    /// <summary>
    ///     Class BaseShardSetActionQueue provides a base implementation for the interface
    ///     <see cref="IShardSetActionQueue" />.
    /// </summary>
    public abstract class BaseShardSetActionQueue : IShardSetActionQueue
    {
        #region constants

        private const string _shardCreationRequest = "ShardCreationRequest";
        private const string _shardDeletionRequest = "ShardDeletionRequest";
        private const string _shardMapPublishingRequest = "ShardMapPublishingRequest";
        private const string _shardMapSyncRequest = "ShardSyncRequest";

        private const string _shardletMoveDelayKey = "ShardletMoveDelay";
        private const string _shardletMoveRequest = "ShardletMoveRequest";

        #endregion

        #region fields

        private readonly Lazy<int> _shardletMoveDelayInMillisecondsLazy = new Lazy<int>(GetShardLetMoveDelay);

        #endregion

        #region properties

        private int ShardletMoveDelayInMilliseconds
        {
            get { return _shardletMoveDelayInMillisecondsLazy.Value; }
        }

        #endregion

        #region IShardSetActionQueue

        /// <summary>
        ///     Check the shard set action queues for the next request and, if found,
        ///     process the associated action off the queue.
        /// </summary>
        /// <param name="uniqueProcessID">A unique process identifier pass to the table driver to identify the caller.</param>
        public virtual void CheckAndProcessQueue(Guid uniqueProcessID)
        {
            SendQueueProcessingEvent("Queue Processing Started");

            //Get the next queue request in order of execution
            var request = GetNextQueueRequest();

            // identity the request type and call the appropriate method to handle the request
            while (request != null)
            {
                switch (request.GetType().Name)
                {
                    case _shardCreationRequest:
                    {
                        SendQueueProcessingEvent(_shardCreationRequest, request);
                        ShardSetConfig.HandleShardCreationRequest(request as ShardCreationRequest, this);
                        break;
                    }
                    case _shardMapPublishingRequest:
                    {
                        SendQueueProcessingEvent(_shardMapPublishingRequest, request);
                        ShardSetConfig.HandleShardMapPublishingRequest(request as ShardMapPublishingRequest,
                            this);
                        break;
                    }
                    case _shardletMoveRequest:
                    {
                        SendQueueProcessingEvent(_shardletMoveRequest, request);
                        Shardlet.HandleShardletMoveRequest(request as ShardletMoveRequest, this, uniqueProcessID);
                        break;
                    }
                    case _shardDeletionRequest:
                    {
                        SendQueueProcessingEvent(_shardDeletionRequest, request);
                        ShardSetConfig.HandleShardDeletionRequest(request as ShardDeletionRequest, this);
                        break;
                    }
                    case _shardMapSyncRequest:
                    {
                        SendQueueProcessingEvent(_shardMapSyncRequest, request);
                        ShardSetConfig.HandleShardSyncRequest(request as ShardSyncRequest, this);
                        break;
                    }
                }

                request = GetNextQueueRequest();
            }

            SendQueueProcessingEvent("Queue Processing Completed");
        }

        /// <summary>
        ///     Gets the queued requests of type TRequest by status.
        /// </summary>
        /// <typeparam name="TRequest">The type of requests to return</typeparam>
        /// <param name="status">The status to filter the queue on.</param>
        /// <returns>IList{TRequest}.</returns>
        public abstract IList<TRequest> GetQueuedRequestsByStatus<TRequest>(TableActionQueueItemStatus status)
            where TRequest : BaseQueueRequest;

        /// <summary>
        ///     Occurs when a request in a queue is processed.
        /// </summary>
        public event QueueProcessingEventHandler QueueProcessingEvent;

        /// <summary>
        ///     Saves the request to queue.
        /// </summary>
        /// <typeparam name="TRequest">The type of requests to save</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>A request of type TRequest</returns>
        public abstract TRequest SaveRequestToQueue<TRequest>(TRequest request) where TRequest : BaseQueueRequest;

        /// <summary>
        ///     Sends a queue processing event.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public virtual void SendQueueProcessingEvent(string message)
        {
            if (QueueProcessingEvent != null)
                QueueProcessingEvent(message);
        }

        #endregion

        #region methods

        /// <summary>
        ///     Are any requests of type TRequest in process.
        /// </summary>
        /// <typeparam name="TRequest">The type of requests to return</typeparam>
        /// <returns><c>true</c> if there are requests of type TRequest in progress, <c>false</c> otherwise.</returns>
        protected abstract bool AreRequestsInProcess<TRequest>() where TRequest : BaseQueueRequest;

        /// <summary>
        ///     Gets the next queue request from the queues.
        /// </summary>
        /// <returns>a subclass of <see cref="BaseQueueRequest" /> representing the next request from the queues</returns>
        protected virtual BaseQueueRequest GetNextQueueRequest()
        {
            //Order for queue orchestration is as follows: 
            //
            // 1) ShardCreationRequests:make the databases that are necessary to handle the rest of the activity
            // 2) ShardMapPublishing: Determine all of the shardlet moves that will be necessary and actually update the shard map in the published store
            // 3) ShardletMoves: Move all of the shardlets to their new homes
            // 4) ShardDeletionRequest: Delete retired shards (which should have been emptied by the steps above);
            // 5) ShardSyncRequest: Apply a SQL statement to a specific shard.

            BaseQueueRequest returnItem = GetNextQueuedRequest<ShardCreationRequest>();
            if (returnItem != null)
                return returnItem;

            returnItem = GetNextQueuedRequest<ShardMapPublishingRequest>();
            if (returnItem != null)
            {
                WaitForEmptyQueue<ShardCreationRequest>();
                return returnItem;
            }

            returnItem = GetNextQueuedRequest<ShardletMoveRequest>();
            if (returnItem != null)
            {
                WaitForEmptyQueue<ShardMapPublishingRequest>();

                if (ShardletMoveDelayInMilliseconds > 0)
                    Thread.Sleep(ShardletMoveDelayInMilliseconds);

                return returnItem;
            }

            returnItem = GetNextQueuedRequest<ShardDeletionRequest>();
            if (returnItem != null)
            {
                WaitForEmptyQueue<ShardletMoveRequest>();
                return returnItem;
            }

            returnItem = GetNextQueuedRequest<ShardSyncRequest>();
            if (returnItem != null)
            {
                WaitForEmptyQueue<ShardDeletionRequest>();
                return returnItem;
            }

            return null;
        }

        /// <summary>
        ///     Gets the next queued request of type TRequest from the queues.
        /// </summary>
        /// <typeparam name="TRequest">The type of request to return</typeparam>
        /// <returns>A request of type T</returns>
        protected abstract TRequest GetNextQueuedRequest<TRequest>() where TRequest : BaseQueueRequest;

        private static int GetShardLetMoveDelay()
        {
            // look in configuration and determine if there is a specified delay between 
            // processing shardlet moves
            var setting = CloudConfigurationManager.GetSetting(_shardletMoveDelayKey);

            int shardletMoveDelay;
            Int32.TryParse(setting, out shardletMoveDelay);

            return shardletMoveDelay;
        }

        private void SendQueueProcessingEvent(string requestName, BaseQueueRequest itemToProcess)
        {
            SendQueueProcessingEvent(string.Format("Processing {0} ({1})", requestName, itemToProcess.QueueId));
        }

        private void WaitForEmptyQueue<T>() where T : BaseQueueRequest
        {
            // Make sure there aren't any in process items for the queue supporting message type T. 
            // If they are, hold on to your queued request until they are done, then process it.
            while (AreRequestsInProcess<T>())
                Thread.Sleep(5000);
        }

        #endregion
    }
}