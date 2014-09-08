#region usings

using System;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages
{
    /// <summary>
    /// Foundational non generic base class for queue items (so we have a constraint for the generic base class)
    /// </summary>
    public abstract class BaseQueueRequest
    {
        #region properties

        /// <summary>
        /// Gets or sets the last touched date and time.
        /// </summary>
        /// <value>The last touched.</value>
        public DateTime LastTouched { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the queue identifier.
        /// </summary>
        /// <value>The queue identifier.</value>
        public long QueueId { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public TableActionQueueItemStatus Status { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseQueueRequest"/> class.
        /// </summary>
        public BaseQueueRequest()
        {
            QueueId = -1;
            Status = TableActionQueueItemStatus.Queued;
        }

        #endregion

        #region methods

        /// <summary>
        /// Saves this instance of the queued request into the queue..
        /// </summary>
        public abstract void Save();

        #endregion
    }

    /// <summary>
    /// Generic base class of Queue items. this allows us to reuse the core code injected from the Queue Manager.
    /// </summary>
    /// <typeparam name="T">The Queue Message Type</typeparam>
    public abstract class BaseQueueRequest<T> : BaseQueueRequest where T : BaseQueueRequest
    {
        #region methods

        /// <summary>
        /// Processes the specified action while tracking the state in the queue.
        /// </summary>
        /// <param name="requestAction">The action to process.</param>
        /// <param name="success">The action to take on successful completion of the requestAction.</param>
        /// <param name="error">The action to take on a failure to completion of the requestAction.</param>
        public void Process(Action<T> requestAction, Action<T> success, Action<T> error)
        {
            try
            {
                Status = TableActionQueueItemStatus.InProcess;
                Save();
                requestAction.Invoke(this as T);
                Status = TableActionQueueItemStatus.Completed;
                Save();
                success.Invoke(this as T);
            }
            catch (Exception ex)
            {
                // todo: log
                Message = ex.Message;
                Status = TableActionQueueItemStatus.Errored;
                Save();

                error.Invoke(this as T);
            }
        }

        /// <summary>
        /// Saves this instance of the queued request into the queue..
        /// </summary>
        public override void Save()
        {
            ScaleOutQueueManager.GetManager().GetQueue().SaveRequestToQueue(this as T);
        }

        #endregion
    }
}