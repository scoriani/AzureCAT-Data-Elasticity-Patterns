namespace Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages
{
    public class ShardCreationRequest : BaseQueueRequest<ShardCreationRequest>
    {
        #region properties

        public string Catalog { get; set; }
        public string ServerInstanceName { get; set; }
        public string ShardSetName { get; set; }

        #endregion
    }
}