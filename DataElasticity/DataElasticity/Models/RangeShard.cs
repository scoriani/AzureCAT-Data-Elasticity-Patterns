#region usings



#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    public class RangeShard : ShardBase
    {
        #region properties

        public long HighDistributionKey { get; set; }
        public long LowDistributionKey { get; set; }
        public int ShardID { get; set; }

        #endregion

        #region constructors

        public RangeShard()
        {
            ShardID = -1;
        }

        #endregion
    }
}