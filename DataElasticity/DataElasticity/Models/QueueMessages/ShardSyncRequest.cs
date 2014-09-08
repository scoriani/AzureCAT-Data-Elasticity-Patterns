#region usings



#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages
{
    /// <summary>
    /// Class ShardSyncRequest represents an action that needs to be executed
    /// against each shard in the scaled out database.
    /// </summary>
    public class ShardSyncRequest : BaseQueueRequest<ShardSyncRequest>
    {
        #region properties

        /// <summary>
        /// Gets or sets the catalog to execute the request against.
        /// </summary>
        /// <value>The catalog.</value>
        public string Catalog { get; set; }

        /// <summary>
        /// Gets or sets the name of the server instance.
        /// </summary>
        /// <value>The name of the server instance.</value>
        public string ServerInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the shard set.
        /// </summary>
        /// <value>The name of the shard set.</value>
        public string ShardSetName { get; set; }

        #endregion
    }
}