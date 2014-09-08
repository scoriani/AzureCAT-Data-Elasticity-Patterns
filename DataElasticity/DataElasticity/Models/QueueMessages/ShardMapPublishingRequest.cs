namespace Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages
{
    /// <summary>
    /// Class ShardMapPublishingRequest defines a request to update the shard map by calculating any moves to align the shartlets
    /// and, optionally updating the ranges.
    /// </summary>
    public class ShardMapPublishingRequest : BaseQueueRequest<ShardMapPublishingRequest>
    {
        #region properties

        /// <summary>
        /// Gets or sets the current shard map identifier.
        /// </summary>
        /// <value>The current shard map identifier.</value>
        public int CurrentShardMapID { get; set; }

        /// <summary>
        /// Gets or sets the new shard map identifier.
        /// </summary>
        /// <value>The new shard map identifier.  Value less than 0 indicates the shard map should not be updated</value>
        public int NewShardMapID { get; set; }

        /// <summary>
        /// Gets or sets the name of the shard set.
        /// </summary>
        /// <value>The name of the shard set.</value>
        public string ShardSetName { get; set; }

        /// <summary>
        /// Gets a value indicating whether the shard map should be updated after publishing the moves.
        /// </summary>
        /// <value><c>true</c> if the shard map should be updated after publishing the moves; otherwise, <c>false</c>.</value>
        public bool ShouldUpdateShardMap
        {
            get { return NewShardMapID > CurrentShardMapID; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShardMapPublishingRequest"/> class.
        /// </summary>
        public ShardMapPublishingRequest()
        {
            NewShardMapID = -1;
        }

        #endregion
    }
}