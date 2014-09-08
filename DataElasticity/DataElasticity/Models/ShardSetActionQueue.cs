using System.Collections.Generic;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    public class ShardSetActionQueue
    {
        #region methods

        public static IShardSetActionQueue GetQueue()
        {
            return ScaleOutQueueManager.GetManager().GetQueue();
        }

        public static IList<TRequest> GetQueuedRequestsByStatus<TRequest>(TableActionQueueItemStatus filter)
            where TRequest : BaseQueueRequest
        {
            return GetQueue().GetQueuedRequestsByStatus<TRequest>(filter);
        }

        #endregion
    }
}