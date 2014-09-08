namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Queues
{
    public class AzureShardAction : BaseQueuedActionEntity
    {
        #region properties

        public string Catalog { get; set; }
        public string ServerInstanceName { get; set; }
        public string ShardSetName { get; set; }

        #endregion

        #region constructors

        public AzureShardAction(string partitionKey, string rowKey) :
            base(partitionKey, rowKey)
        {
        }

        public AzureShardAction()
        {
        }

        #endregion
    }
}