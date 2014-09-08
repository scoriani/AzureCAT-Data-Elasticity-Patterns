namespace Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages
{
    public class ShardDeletionRequest : BaseQueueRequest<ShardDeletionRequest>
    {
        #region properties

        public string Catalog { get; set; }
        public string ServerInstanceName { get; set; }
        public string ShardSetName { get; set; }

        #endregion
    }
}