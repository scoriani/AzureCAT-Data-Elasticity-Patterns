// ***********************************************************************

#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Shards;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels
{
    /// <summary>
    /// Class CacheRangeShard is the serializable object held in the object cache for range shards.
    /// </summary>
    [Serializable]
    public class CacheRangeShard
    {
        #region properties

        /// <summary>
        /// Gets or sets the catalog.
        /// </summary>
        /// <value>The catalog.</value>
        public string Catalog { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Gets or sets the maximum range.
        /// </summary>
        /// <value>The maximum range.</value>
        public long MaxRange { get; set; }

        /// <summary>
        /// Gets or sets the name of the server instance.
        /// </summary>
        /// <value>The name of the server instance.</value>
        public string ServerInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the shard set.
        /// </summary>
        /// <value>The name of the shard set.</value>
        public string ShardSetName { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Convert to the the Azure Table Storage model for the range shard.
        /// </summary>
        /// <returns>AzureRangeShard.</returns>
        public AzureRangeShard ToAzureRangeShard()
        {
            if (IsEmpty)
            {
                return new AzureRangeShard();
            }
            return new AzureRangeShard
            {
                Catalog = Catalog,
                MaxRange = MaxRange,
                ServerInstanceName = ServerInstanceName,
                ShardSetName = ShardSetName
            };
        }

        #endregion
    }
}