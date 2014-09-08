#region usings

using System;
using System.Collections.Generic;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Interfaces
{
    /// <summary>
    /// Delegate for a queue event.
    /// </summary>
    /// <param name="message">Message to be bubbled up.</param>
    public delegate void QueueProcessingEventHandler(string message);

    /// <summary>
    /// This is the interface to your queue provider for handling large tasks like publishing a new Shard map 
    /// (which could cause large quantities of ShardLet moves or other complexities).
    /// </summary>
    public interface IShardSetActionQueue
    {
        #region events

        /// <summary>
        /// Occurs when a request in a queue is processed.
        /// </summary>
        event QueueProcessingEventHandler QueueProcessingEvent;

        #endregion

        #region methods

        /// <summary>
        /// Check the shard set action queues for the next request and, if found,
        /// process the associated action off the queue.
        /// </summary>
        /// <param name="uniqueProcessID">A unique process identifier pass to the table driver to identify the caller.</param>
        void CheckAndProcessQueue(Guid uniqueProcessID);

        /// <summary>
        /// Gets the queued requests of type TRequest by status.
        /// </summary>
        /// <typeparam name="TRequest">The type of requests to return</typeparam>
        /// <param name="status">The status to filter the queue on.</param>
        /// <returns>IList{TRequest}.</returns>
        IList<TRequest> GetQueuedRequestsByStatus<TRequest>(TableActionQueueItemStatus status)
            where TRequest : BaseQueueRequest;

        /// <summary>
        /// Saves the request to queue.
        /// </summary>
        /// <typeparam name="TRequest">The type of requests to save</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>A request of type TRequest</returns>
        TRequest SaveRequestToQueue<TRequest>(TRequest request) where TRequest : BaseQueueRequest;

        /// <summary>
        /// Sends a queue processing event.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void SendQueueProcessingEvent(string message);

        #endregion
    }
}