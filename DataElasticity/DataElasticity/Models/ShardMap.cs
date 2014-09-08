#region usings

using System.Collections.Generic;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    public class ShardMap
    {
        #region properties

        public int ShardMapID { get; set; }
        public IList<RangeShard> Shards { get; set; }

        #endregion

        #region constructors

        public ShardMap()
        {
            Shards = new List<RangeShard>();
            ShardMapID = -1;
        }

        #endregion

        #region methods

        public static ShardMap Load(int shardMapID)
        {
            return ScaleOutConfigManager.GetManager().GetShardMap(shardMapID);
        }

        #endregion
    }
}