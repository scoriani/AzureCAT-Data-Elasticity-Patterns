#region usings

using System.Globalization;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels;
using Microsoft.WindowsAzure.Storage.Table;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Shards
{
    /// <summary>
    /// Class AzureShardletConnection is the data model class for saving data into the shardlet connections in azure table storage.
    /// </summary>
    public class AzureShardletConnection : TableEntity
    {
        #region fields

        private long _distributionKey;
        private int _spid;

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the catalog.
        /// </summary>
        /// <value>The catalog.</value>
        public string Catalog { get; set; }

        /// <summary>
        /// Gets or sets the name of the shard set.
        /// </summary>
        /// <value>The name of the shard set.</value>
        public long DistributionKey
        {
            get { return _distributionKey; }
            set
            {
                _distributionKey = value;
                PartitionKey = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets or sets the sharding key.
        /// </summary>
        /// <value>The sharding key.</value>
        public string ShardingKey { get; set; }

        /// <summary>
        /// Gets or sets the distribution key.
        /// </summary>
        /// <value>The distribution key.</value>
        public int Spid
        {
            get { return _spid; }
            set
            {
                _spid = value;
                SetRowKey();
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureShardletConnection"/> class.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        public AzureShardletConnection(string partitionKey, string rowKey) :
            base(partitionKey, rowKey)
        {
            ETag = "*";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureShardletConnection"/> class.
        /// </summary>
        public AzureShardletConnection()
        {
            ETag = "*";
        }

        #endregion

        #region methods

        /// <summary>
        /// Gets the row key value from a Catalog and SPID.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="spid">The spid.</param>
        /// <returns>System.String.</returns>
        public static string GetRowKey(string catalog, int spid)
        {
            if (string.IsNullOrWhiteSpace(catalog) || spid <= 0)
            {
                return null;
            }
            return string.Format("{0}_{1}", catalog, spid);
        }

        private void SetRowKey()
        {
            RowKey = GetRowKey(Catalog, _spid);
        }

        /// <summary>
        /// Convert to the the Azure Cache model for azure shardlet connections.
        /// </summary>
        /// <returns>AzureShardletConnection.</returns>
        public CacheShardletConnection ToCacheShardletConnection()
        {
            return new CacheShardletConnection
            {
                Catalog = Catalog,
                DistributionKey = DistributionKey,
                ShardingKey = ShardingKey,
                Spid = Spid,
            };
        }

        #endregion
    }
}