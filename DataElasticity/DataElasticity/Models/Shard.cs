#region usings

using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    public class Shard : ShardBase
    {
        #region properties

        public string Description { get; set; }
        public int PointerShardID { get; set; }

        #endregion

        #region constructors

        public Shard()
        {
            PointerShardID = -1;
        }

        #endregion
    }
}