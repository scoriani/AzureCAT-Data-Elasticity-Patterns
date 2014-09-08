#region usings

using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity
{
    /// <summary>
    /// This is just a utility class to grab the configured implementation of the IShardSetActionQueue
    /// </summary>
    internal class ScaleOutQueueManager : UnityBasedManager<ScaleOutQueueManager>
    {
        #region fields

        private readonly IShardSetActionQueue _shardSetActionQueue;

        #endregion

        #region constructors

        public ScaleOutQueueManager(IShardSetActionQueue shardSetActionQueue)
        {
            _shardSetActionQueue = shardSetActionQueue;
        }

        #endregion

        #region methods

        public IShardSetActionQueue GetQueue()
        {
            return _shardSetActionQueue;
        }

        #endregion
    }
}