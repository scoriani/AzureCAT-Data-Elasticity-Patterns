using System;

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages
{
    public class ShardletMoveRequest : BaseQueueRequest<ShardletMoveRequest>
    {
        #region properties

        public bool DeleteOnMove { get; set; }
        public string DestinationCatalog { get; set; }
        public string DestinationServerInstanceName { get; set; }
        public long DistributionKey { get; set; }
        public string ShardingKey { get; set; }
        public bool Pin { get; set; }
        public string ShardSetName { get; set; }
        public string SourceCatalog { get; set; }
        public string SourceServerInstanceName { get; set; }
        public Guid UniqueProcessID { get; set; }

        #endregion
    }
}