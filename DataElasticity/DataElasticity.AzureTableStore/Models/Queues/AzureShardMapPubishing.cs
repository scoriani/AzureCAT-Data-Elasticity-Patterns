namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Queues
{
    public class ShardMapPublishing : BaseQueuedActionEntity
    {
        #region properties

        public int NewShardMapID { get; set; }
        public int CurrentShardMapID { get; set; }
        public string ShardSetName { get; set; }

        #endregion

        #region constructors

        public ShardMapPublishing(string partitionKey, string rowKey) :
            base(partitionKey, rowKey)
        {
        }

        public ShardMapPublishing()
        {
        }

        #endregion
    }
}