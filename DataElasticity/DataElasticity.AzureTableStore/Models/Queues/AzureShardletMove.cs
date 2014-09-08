namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Queues
{
    public class AzureShardletMove : BaseQueuedActionEntity
    {
        #region properties

        public bool DeleteOnMove { get; set; }
        public string DestinationCatalog { get; set; }
        public string DestinationServerInstanceName { get; set; }
        public bool Pin { get; set; }
        public string SourceCatalog { get; set; }
        public string SourceServerInstanceName { get; set; }
        public long DistributionKey { get; set; }
        public string ShardingKey { get; set; }
        public string ShardSetName { get; set; }

        #endregion

        #region constructors

        public AzureShardletMove(string partitionKey, string rowKey) :
            base(partitionKey, rowKey)
        {
        }

        public AzureShardletMove()
        {
        }

        #endregion
    }
}