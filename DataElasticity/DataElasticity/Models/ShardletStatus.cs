namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    /// <summary>
    /// Enum ShardletStatus defines the current status of the ShardLet returned
    /// to a user of the ShardLet connection.
    /// </summary>
    public enum ShardletStatus
    {
        /// <summary>
        /// The Shardlet is active and can support full operations.
        /// </summary>
        Active,

        /// <summary>
        /// The Shardlet is undergoing maintenance of some kind.
        /// </summary>
        Moving
    }
}